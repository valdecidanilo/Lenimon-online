using System;
using System.Collections.Generic;
using System.Text;
using Battle;
using Battle.OpponentAI;
using UnityEngine;
using LenixSO.Logger;
using Random = UnityEngine.Random;
using Logger = LenixSO.Logger.Logger;

public class GameManager : MonoBehaviour
{
    private const int MaxPokemonId = 1025;

    [SerializeField] private BattleSetup battle;
    [SerializeField] private BattleMode battleMode = BattleMode.WildEncounter;

    private Trainer player;
    private Trainer opponent;
    private Checklist itemsLoaded;
    private int encounterLevel;
    public static Action OnInitializeTest;

    private void OnEnable()
    {
        OnInitializeTest += InitializeBattle;
    }

    private void InitializeBattle()
    {
        PokeDatabase.PreloadAssets();
        LoadingScreen.onDoneLoading += SetupBattleMode;
    }
    
    private void SetupBattleMode()
    {
        LoadingScreen.onDoneLoading -= SetupBattleMode;
        switch (battleMode)
        {
            case BattleMode.WildEncounter:
                SetupWildBattle();
                break;
            case BattleMode.TrainerAI:
                SetupTrainerBattle(new FullRandomAI());
                break;
            case BattleMode.PlayerVsPlayer:
                SetupPvPBattle();
                break;
            default:
                SetupWildBattle();
                break;
        }
    }
    
    private void SetupWildBattle()
    {
        player = CreateDefaultPlayer("You"); // tem que fazer o load do usuario
        opponent = new WildOpponent();
        Logger.Log("WILD BATTLE");
        GenerateParties(true);
    }

    private void SetupTrainerBattle(Opponent trainer)
    {
        player = CreateDefaultPlayer("You"); // tem que fazer o load do usuario
        opponent = trainer;
        GenerateParties();
        LoadingScreen.onDoneLoading += StartBattle;
    }

    private void SetupPvPBattle()
    {
        player = CreateDefaultPlayer("You");
        opponent = MockRemoteOpponentPlayer();
        LoadingScreen.onDoneLoading += () => battle.SetupBattle(player, (Opponent) opponent);
    }

    private Trainer CreateDefaultPlayer(string name)
    {
        return new Trainer()
        {
            name = name,
            referenceText = "Allied"
        };
    }
    
    private void StartBattle()
    {
        if (opponent?.party == null)
        {
            Logger.LogError("opponent.party está null! A batalha não será iniciada.");
            return;
        }

        if (opponent.party.Length == 0)
        {
            Logger.LogError("opponent.party está vazio!");
            return;
        }
        
        battle.gameObject.SetActive(true);
        battle.SetupBattle(player, (Opponent) opponent);
    }
    #region Online Test
    
    private Trainer MockRemoteOpponentPlayer()
    {
        
        var mockOpponent = new MockRemotePlayerAI
        {
            party = new Pokemon[3]
        };

        Checklist loaded = new(mockOpponent.party.Length);

        for (var i = 0; i < mockOpponent.party.Length; i++)
        {
            var id = Random.Range(1, 152);
            var level = Random.Range(10, 30);
            GetPokemon(id, level, (pokemon) =>
            {
                mockOpponent.party[loaded.currentSteps] = pokemon;
                loaded.FinishStep();
            });
        }

        loaded.onCompleted += () =>
        {
            mockOpponent.activePokemon = mockOpponent.party[0];
            battle.SetupBattle(player, mockOpponent);
        };

        return mockOpponent;
    }
    
    #endregion
    
    #region Pokemon
    private void GenerateParties(bool generateOpponent = true)
    {
        encounterLevel = Random.Range(1, 101);

        // ====== Gerar party do PLAYER ======
        var playerPartySize = Random.Range(1, 7);
        var playerPartyLevel = OptionsMenu.battleLevel ?? Mathf.Min(100, encounterLevel + (6 - playerPartySize));
        player.party = new Pokemon[playerPartySize];
        Checklist alliesLoaded = new(playerPartySize);

        Logger.Log($"setup ally party ({playerPartySize} pokemons)", LogFlags.Game);
        LoadingScreen.AddOrChangeQueue(alliesLoaded, $"Loading ally party[1/{alliesLoaded.requiredSteps}]...");

        alliesLoaded.onProgress += (p) => LoadingScreen.AddOrChangeQueue(alliesLoaded,
            $"Loading ally party[{alliesLoaded.currentSteps + 1}/{alliesLoaded.requiredSteps}]...");

        alliesLoaded.onCompleted += () =>
        {
            Logger.Log($"generateOpponent = {generateOpponent}", LogFlags.Tests);
            if (!generateOpponent)
            {
                LoadingScreen.onDoneLoading += StartBattle;
                return;
            }
            Logger.Log("INICIANDO GERAÇÃO DO OPONENTE!", LogFlags.Tests);
            var isWild = opponent is WildOpponent;
            var opponentPartySize = isWild ? 1 : Random.Range(1, 7);
            var opponentPartyLevel = OptionsMenu.battleLevel ?? Mathf.Min(100, encounterLevel + (6 - opponentPartySize));
            opponent.party = new Pokemon[opponentPartySize];

            var opponentLoaded = new Checklist(opponentPartySize);
            LoadingScreen.AddOrChangeQueue(opponentLoaded, "Loading opponent party...");

            opponentLoaded.onProgress += (p) =>
            {
                opponent.activePokemon ??= opponent.party[0];
                if (p == 0) GenerateItems();
            };

            opponentLoaded.onCompleted += () =>
            {
                StringBuilder sb = new("Opponent's party:");
                foreach (var t in opponent.party)
                    sb.Append($"\n{t.name}");
                Logger.Log(sb.ToString(), LogFlags.DataCheck);
                LoadingScreen.onDoneLoading += StartBattle;
            };

            Logger.Log($"setup enemy party ({opponentPartySize} pokemons)", LogFlags.Game);
            SetupParty(opponent.party, opponentPartyLevel, opponentLoaded);
        };

        SetupParty(player.party, playerPartyLevel, alliesLoaded);

        void SetupParty(Pokemon[] party, int level, Checklist loaded)
        {
            if (loaded.isDone) return;
            var pokemonId = Random.Range(1, MaxPokemonId + 1);
            GetPokemon(pokemonId, level, (pokemon) =>
            {
                CheckPokemon(pokemon);
                party[loaded.currentSteps] = pokemon;
                loaded.FinishStep();
                SetupParty(party, level, loaded);
            });
        }
    }
    private static void GetPokemon(int pokemonId, int level, Action<Pokemon> onFinished)
    {
        PokeAPI.GetPokemonData(pokemonId, (data) => { Pokemon.GetLoadedPokemon(data, level, onFinished); });
    }
    private static void CheckPokemon(Pokemon pokemon)
    {
        StringBuilder finalLog = new();
        StringBuilder type = new();
        for (int i = 0; i < pokemon.data.types.Count; i++)
        {
            type.Append(pokemon.data.types[i].type.name);
            if (i < pokemon.data.types.Count - 1) type.Append("\\");
        }

        StringBuilder abilities = new();
        for (int i = 0; i < pokemon.data.abilities.Count; i++)
        {
            AbilityReference ability = pokemon.data.abilities[i];
            abilities.Append($"{ability.reference.name}");
            if (ability.hidden) abilities.Append("(H)");
            if (i < pokemon.data.abilities.Count - 1) abilities.Append(" | ");
        }

        finalLog.Append($"{pokemon.gender} {pokemon.name} (no.{pokemon.id}) -> Lv{pokemon.level}");
        finalLog.Append($"\n{type}");
        finalLog.Append($"\n{abilities}");

        Logger.Log(finalLog.ToString(), LogFlags.DataCheck);
    }
    #endregion

    #region Items
    private void GenerateItems()
    {
        itemsLoaded = new(0);
        GenerateHealItems();
        GenerateBattleItems();
        GenerateTMs();
        itemsLoaded.AddStep();
        opponent.SetupBag(encounterLevel, () => itemsLoaded.FinishStep());
        LoadingScreen.AddOrChangeQueue(itemsLoaded, "Loading items...");
    }

    private void GenerateHealItems()
    {
        itemsLoaded.AddStep();
        List<(string name,int amount)> itemList = new()
        {
            ("revive", player.party.Length),
            ("potion",6),
            ("super-potion", 5),
            ("soda-pop", 4),
            ("lemonade", 3),
            ("moomoo-milk", 2),
            ("hyper-potion", 1),
            ("max-potion", 1)
        };

        Checklist loaded = new(itemList.Count);
        StringBuilder log = new($"loaded heal items are:");
        LoadHealItem();

        void LoadHealItem()
        {
            var route = $"{PokeAPI.baseRoute}item/{itemList[loaded.currentSteps].name}";
            PokeAPI.GetItem(route, (item) =>
            {
                item.amount = itemList[loaded.currentSteps].amount;
                player.bag.items.Add(item);
                item.battleEffect = ItemEffect.GenerateItemEffect(item);
                log.Append($"\n{item.name}");
                loaded.FinishStep();
                if (loaded.isDone)
                {
                    Logger.Log(log.ToString(), LogFlags.DataCheck);
                    itemsLoaded.FinishStep();
                    return;
                }
                LoadHealItem();
            });
        }
        
        itemsLoaded.FinishStep();
    }

    private void GenerateBattleItems()
    {
        itemsLoaded.AddStep();
        List<string> itemList = new()
        {
            "x-attack",
            "x-defense",
            "x-sp-atk",
            "x-sp-def",
            "x-speed",
            "x-accuracy"
        };

        Checklist loaded = new(itemList.Count);
        StringBuilder log = new($"loaded battle items are:");
        LoadBattleItem();

        void LoadBattleItem()
        {
            var route = $"{PokeAPI.baseRoute}item/{itemList[loaded.currentSteps]}";
            PokeAPI.GetItem(route, (item) =>
            {
                item.amount = 10;
                player.bag.battleItems.Add(item);
                item.battleEffect = ItemEffect.GenerateItemEffect(item);
                log.Append($"\n{item.name}");
                loaded.FinishStep();
                if (loaded.isDone)
                {
                    Logger.Log(log.ToString(), LogFlags.DataCheck);
                    itemsLoaded.FinishStep();
                    return;
                }
                LoadBattleItem();
            });
        }
    }

    private void GenerateTMs()
    {
        itemsLoaded.AddStep();
        const int tmPerPokemon = 2;
        List<string> itemList = new(player.party.Length * tmPerPokemon);
        //{
        //    "solar-beam",
        //    "earthquake",
        //};
        
        List<MoveReference> TMs = new(itemList.Count);
        //for (int i = 0; i < TMs.Capacity; i++)
        //    TMs.Add(new() { move = new() { url = $"{PokeAPI.baseRoute}move/{itemList[i]}" } });
        
        for (var i = 0; i < player.party.Length; i++)
        {
            var pokemon = player.party[i];
            var moves = MoveHelper.GetPossibleMoves(pokemon, new [] { MoveLearnMethod.TM });
            List<MoveReference> learnableMoves = new();
            for (var j = 0; j < moves.Count; j++)
            {
                var moveData = moves[j];
                if (itemList.Contains(moveData.move.name)) continue;
                //check if pokemon doesn't already know this move
                for (var k = 0; k < pokemon.moves.Length; k++)
                {
                    var move = pokemon.moves[k];
                    if (moveData.move.name == 
                        move?.Data?.name) continue;
                    learnableMoves.Add(moveData);
                }
            }
            //get 2(?) random moves
            var movesToAdd = Mathf.Min(tmPerPokemon, learnableMoves.Count);
            for (var j = 0; j < movesToAdd; j++)
            {
                var randomId = Random.Range(0, learnableMoves.Count);
                itemList.Add(learnableMoves[randomId].move.name);
                TMs.Add(learnableMoves[randomId]);
                learnableMoves.RemoveAt(randomId);
            }
        }

        //TMs = MoveHelper.GetPossibleMoves(allyParty[0], new[] { MoveLearnMethod.TM });
        Checklist loadedTMs = new(TMs.Count);
        StringBuilder log = new($"loaded TMs are:");
        if (TMs.Count > 0) LoadTMs(TMs[0]);
        void LoadTMs(MoveReference moveReference)
        {
            PokeAPI.GetMoveData(moveReference.move.url, LoadMoveData);

            void LoadMoveData(MoveData moveData)
            {
                PokeAPI.GetTM(moveData, (tm) =>
                {
                    player.bag.TMs.Add(tm);
                    tm.battleEffect = MoveEffectCreator.EmptyEffect();
                    log.Append($"\n{tm.name} => {tm.data.moveData.name}");
                    loadedTMs.FinishStep();
                    if (!loadedTMs.isDone)
                    {
                        LoadTMs(TMs[loadedTMs.currentSteps]);
                        return;
                    }
                    Logger.Log(log.ToString(), LogFlags.DataCheck);
                    itemsLoaded.FinishStep();
                });
            }
        }
    }
    #endregion
}