using System;
using System.Collections.Generic;
using System.Text;
using Battle;
using UnityEngine;
using LenixSO.Logger;
using Random = UnityEngine.Random;
using Logger = LenixSO.Logger.Logger;

public class GameManager : MonoBehaviour
{
    private const int MaxPokemonId = 1025;

    [SerializeField] private BattleSetup battle;

    private Trainer player;
    private Opponent opponent;
    private int encounterLevel;

    private Checklist itemsLoaded;

    private void Awake()
    {
        PokeDatabase.PreloadAssets();
        LoadingScreen.onDoneLoading += Setup;
    }

    private void Setup()
    {
        player = new()
        {
            name = "You",
            referenceText = "Allied"
        };

        opponent = new FullRandomAI()
        {
            name = "Opponent",
            referenceText = "Opponent's"
        };

        GenerateParties();
        LoadingScreen.onDoneLoading -= Setup;
        LoadingScreen.onDoneLoading += StartBattle;
    }
    private void StartBattle()
    {
        battle.SetupBattle(player, opponent);
    }

    #region Pokemon
    private void GenerateParties()
    {
        //get encounter level
        encounterLevel = Random.Range(1, 101);

        //generate ally party
        int partySize = Random.Range(1, 7);
        int partyLevel = OptionsMenu.battleLevel ?? Mathf.Min(100, encounterLevel + (6 - partySize));
        player.party = new Pokemon[partySize];
        Checklist alliesLoaded = new(partySize);

        Checklist requiredEnemy = new(1);
        Logger.Log($"setup ally party ({partySize} pokemons)", LogFlags.Game);
        LoadingScreen.AddOrChangeQueue(alliesLoaded, $"Loading ally party[1/{alliesLoaded.requiredSteps}]...");
        LoadingScreen.AddOrChangeQueue(requiredEnemy, $"Loading opponent party...");

        alliesLoaded.onProgress += (p) => LoadingScreen.AddOrChangeQueue(alliesLoaded,
            $"Loading ally party[{alliesLoaded.currentSteps + 1}/{alliesLoaded.requiredSteps}]...");
        alliesLoaded.onCompleted += () =>
        {
            //load enemies
            partySize = Random.Range(1, 7);
            partyLevel = OptionsMenu.battleLevel ?? Mathf.Min(100, encounterLevel + (6 - partySize));
            opponent.party = new Pokemon[partySize];

            Checklist opponentLoaded = new(partySize);
            opponentLoaded.onProgress += (p) =>
            {
                if (requiredEnemy.isDone) return;
                opponent.activePokemon = opponent.party[0];
                GenerateItems();
                requiredEnemy.FinishStep();
            };
            opponentLoaded.onCompleted += () =>
            {
                StringBuilder sb = new("Opponent's party:");
                for (int i = 0; i < opponent.party.Length; i++)
                {
                    sb.Append($"\n{opponent.party[i].name}");
                }
                Logger.Log(sb.ToString(), LogFlags.DataCheck);
            };
            Logger.Log($"setup enemy party ({partySize} pokemons)", LogFlags.Game);
            SetupParty(opponent.party, opponentLoaded);
        };
        SetupParty(player.party, alliesLoaded);

        void SetupParty(Pokemon[] party, Checklist loaded)
        {
            if (loaded.isDone) return;
            int pokemonId = Random.Range(1, MaxPokemonId + 1);
            GetPokemon(pokemonId, partyLevel, (pokemon) =>
            {
                CheckPokemon(pokemon);
                party[loaded.currentSteps] = pokemon;
                loaded.FinishStep();
                SetupParty(party, loaded);
            });
        }
    }
    private void GetPokemon(int pokemonId, int level, Action<Pokemon> onFinished)
    {
        PokeAPI.GetPokemonData(pokemonId, (data) => { Pokemon.GetLoadedPokemon(data, level, onFinished); });
    }
    private void CheckPokemon(Pokemon pokemon)
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
            string route = $"{PokeAPI.baseRoute}item/{itemList[loaded.currentSteps].name}";
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
            string route = $"{PokeAPI.baseRoute}item/{itemList[loaded.currentSteps]}";
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
        
        for (int i = 0; i < player.party.Length; i++)
        {
            Pokemon pokemon = player.party[i];
            var moves = MoveHelper.GetPossibleMoves(pokemon, new [] { MoveLearnMethod.TM });
            List<MoveReference> learnableMoves = new();
            for (int j = 0; j < moves.Count; j++)
            {
                var moveData = moves[j];
                //check if tm was already added
                if (itemList.Contains(moveData.move.name)) continue;
                //check if pokemon doesn't already know this move
                for (int k = 0; k < pokemon.moves.Length; k++)
                {
                    var move = pokemon.moves[k];
                    if (moveData.move.name == 
                        move?.Data?.name) continue;
                    learnableMoves.Add(moveData);
                }
            }
            //get 2(?) random moves
            int movesToAdd = Mathf.Min(tmPerPokemon, learnableMoves.Count);
            for (int j = 0; j < movesToAdd; j++)
            {
                int randomId = Random.Range(0, learnableMoves.Count);
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