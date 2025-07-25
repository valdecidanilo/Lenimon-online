using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Auth;
using Battle;
using Battle.OpponentAI;
using DB;
using DB.Data;
using UnityEngine;
using LenixSO.Logger;
using Newtonsoft.Json;
using Player;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Logger = LenixSO.Logger.Logger;

public class GameManager : MonoBehaviour
{
    private const int MaxPokemonId = 1025;

    [SerializeField] private BattleSetup battle;
    [SerializeField] private BattleMode battleMode = BattleMode.WildEncounter;

    [SerializeField] private PlayerEntity currentPlayer;
    [SerializeField] private UserData userData;
    private Trainer opponent;
    private Checklist itemsLoaded;
    private int encounterLevel;
    public static Action OnInitializeTest;

    public Button testeRegister;
    public Button testeLogin;
    public AuthController auth;
    private void OnEnable()
    {
        OnInitializeTest += InitializeBattle;
        testeRegister.onClick.AddListener(RegisterFake);
        testeLogin.onClick.AddListener(LoginFake);
    }

    private void Awake()
    {
        auth.Database = new Database();
        PokeDatabase.PreloadAssets();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.P)) AddPokemonToUser();
    }

    private void RegisterFake()
    {
        userData = auth.Database.RegisterUser(
            "valdecidanilo1@live.com", 
            "danilo285", 
            "chupivaru", Random.Range(0, 152)).Item3;
        StartCoroutine(TestTime());
    }

    private IEnumerator TestTime()
    {
        yield return new WaitForSeconds(1f);
        LoginFake();
    }

    private void LoginFake()
    {
        userData = auth.Database.LoginUser("valdecidanilo1@live.com", "danilo285");
        var pokemons = auth.Database.GetSplicemonsByUser(userData.Id)
            .OrderBy(p => p.PartyIndex)
            .ToList();

        currentPlayer.trainer.name = userData.Nickname;

        var party = new Pokemon[pokemons.Count];
        Checklist checklist = new(pokemons.Count);

        for (var i = 0; i < pokemons.Count; i++)
        {
            var partyIndex = pokemons[i].PartyIndex;
            var model = pokemons[i];

            Pokemon.GetLoadedPokemon(model, (poke) =>
            {
                party[partyIndex] = poke;
                checklist.FinishStep();
            });
        }

        checklist.onCompleted += () =>
        {
            currentPlayer.trainer.party = party.ToList();
            Debug.Log("Party carregada com sucesso em ordem!");
        };
    }

    public void AddPokemonToUser()
    {
        Debug.Log("adding pokemon to user");
        auth.Database.AddPokemonToUser(userData.Id, Random.Range(0, 152), updatedParty =>
        {
            var newPoke = updatedParty[^1];
            Debug.Log($"Novo Pokémon {newPoke.name.ToUpper()} adicionado e party atualizada!");
            PartyMenu.OnUpdateParty?.Invoke(updatedParty);
        });
    }
    private void InitializeBattle()
    {
        SetupBattleMode();
        battle.OpenBattleScene();
    }
    
    private void SetupBattleMode()
    {
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
        battle.currentPlayer = currentPlayer;
        opponent = new WildOpponent();
        GenerateParties(true);
    }

    private void SetupTrainerBattle(Opponent trainer)
    {
        opponent = trainer;
        GenerateParties();
        LoadingScreen.onDoneLoading += StartBattle;
    }

    private void SetupPvPBattle()
    {
        opponent = MockRemoteOpponentPlayer();
        LoadingScreen.onDoneLoading += () => battle.SetupBattle(currentPlayer.trainer, (Opponent) opponent);
    }
    
    private void StartBattle()
    {
        battle.gameObject.SetActive(true);
        battle.SetupBattle(currentPlayer.trainer, (Opponent) opponent);
    }
    #region Online Test
    
    private Trainer MockRemoteOpponentPlayer()
    {
        
        var mockOpponent = new MockRemotePlayerAI
        {
            party = new List<Pokemon>()
        };

        Checklist loaded = new(mockOpponent.party.Count);

        for (var i = 0; i < mockOpponent.party.Count; i++)
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
            battle.SetupBattle(currentPlayer.trainer, mockOpponent);
        };

        return mockOpponent;
    }
    
    #endregion
    
    #region Pokemon
    private void GenerateParties(bool generateOpponent = true)
    {
        encounterLevel = Random.Range(5, 7);

        if (!generateOpponent)
        {
            LoadingScreen.onDoneLoading += StartBattle;
            return;
        }
        var isWild = opponent is WildOpponent;
        var opponentPartySize = isWild ? 1 : Random.Range(1, 7);
        var opponentPartyLevel = OptionsMenu.battleLevel ?? Mathf.Min(100, encounterLevel + (6 - opponentPartySize));
        opponent.party = new List<Pokemon>();

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

        SetupParty(opponent.party, opponentPartyLevel, opponentLoaded);

        void SetupParty(List<Pokemon> party, int level, Checklist loaded)
        {
            if (loaded.isDone) return;
            var pokemonId = Random.Range(1, MaxPokemonId + 1);
            GetPokemon(pokemonId, level, (pokemon) =>
            {
                CheckPokemon(pokemon);
                party.Add(pokemon);
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
        for (var i = 0; i < pokemon.data.types.Count; i++)
        {
            type.Append(pokemon.data.types[i].type.name);
            if (i < pokemon.data.types.Count - 1) type.Append("\\");
        }

        StringBuilder abilities = new();
        for (var i = 0; i < pokemon.data.abilities.Count; i++)
        {
            var ability = pokemon.data.abilities[i];
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
        itemsLoaded = new Checklist(0);
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
            ("revive", currentPlayer.trainer.party.Count),
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
                currentPlayer.trainer.bag.items.Add(item);
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
                currentPlayer.trainer.bag.battleItems.Add(item);
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
        List<string> itemList = new(currentPlayer.trainer.party.Count * tmPerPokemon);
        //{
        //    "solar-beam",
        //    "earthquake",
        //};
        
        List<MoveReference> TMs = new(itemList.Count);
        //for (int i = 0; i < TMs.Capacity; i++)
        //    TMs.Add(new() { move = new() { url = $"{PokeAPI.baseRoute}move/{itemList[i]}" } });
        
        for (var i = 0; i < currentPlayer.trainer.party.Count; i++)
        {
            var pokemon = currentPlayer.trainer.party[i];
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
                    currentPlayer.trainer.bag.TMs.Add(tm);
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