using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Battle;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using LenixSO.Logger;
using Random = UnityEngine.Random;
using Logger = LenixSO.Logger.Logger;

public class TestScript : MonoBehaviour
{
    private const int maxPokemonId = 1025;

    [SerializeField] private BattleSetup battle;

    private Pokemon[] allyParty;
    private Pokemon[] enemyParty;

    private Checklist itemsLoaded;

    private void Awake()
    {
        PokeDatabase.PreloadAssets();
        LoadingScreen.onDoneLoading += Setup;
    }

    private void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame) SceneManager.LoadScene(0);
    }

    private void Setup()
    {
        LoadingScreen.onDoneLoading -= Setup;
        LoadingScreen.onDoneLoading += GenerateItems;
        GenerateParties();
    }

    #region Pokemon
    private void GenerateParties()
    {
        //get encounter level
        int encounterLevel = Random.Range(1, 101);

        //generate ally party
        int partySize = Random.Range(1, 7);
        int partyLevel = Mathf.Min(100, encounterLevel + (6 - partySize));
        allyParty = new Pokemon[partySize];
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
            partyLevel = Mathf.Min(100, encounterLevel + (6 - partySize));
            enemyParty = new Pokemon[partySize];

            Checklist opponentLoaded = new(partySize);
            opponentLoaded.onProgress += (p) =>
            {
                if (!requiredEnemy.isDone) requiredEnemy.FinishStep();
            };
            Logger.Log($"setup enemy party ({partySize} pokemons)", LogFlags.Game);
            SetupParty(enemyParty, opponentLoaded);
        };
        SetupParty(allyParty, alliesLoaded);

        void SetupParty(Pokemon[] party, Checklist loaded)
        {
            if (loaded.isDone) return;
            int pokemonId = Random.Range(1, maxPokemonId + 1);
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
    private void StartBattle()
    {
        battle.SetupBattle(allyParty, enemyParty);
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
        finalLog.Append($"\n{type.ToString()}");
        finalLog.Append($"\n{abilities.ToString()}");

        Logger.Log(finalLog.ToString(), LogFlags.DataCheck);
    }
    #endregion

    #region Items
    private void GenerateItems()
    {
        LoadingScreen.onDoneLoading -= GenerateItems;
        LoadingScreen.onDoneLoading += StartBattle;
        itemsLoaded = new(3);
        LoadingScreen.AddOrChangeQueue(itemsLoaded, "Loading items...");
    }

    private void GenerateHealItems()
    {
        List<string> itemList = new();
        itemList.Add("potion");
        itemList.Add("super-potion");
        itemList.Add("hyper-potion");
        itemList.Add("max-potion");
        
        itemsLoaded.FinishStep();
    }

    private void GenerateBattleItems()
    {
        List<string> itemList = new();
        itemList.Add("x-attack");
        itemList.Add("x-defense");
        itemList.Add("x-sp-atk");
        itemList.Add("x-sp-def");
        itemList.Add("x-speed");
        itemList.Add("x-accuracy");
        
        itemsLoaded.FinishStep();
    }

    private void GenerateTMs()
    {
        int tmPerPokemon = 4;
        List<string> itemList = new();
        itemList.Add("solar-beam");
        itemList.Add("earthquake");

        List<MoveReference> possibleTMs = MoveHelper.GetPossibleMoves(allyParty[0], new[] { MoveLearnMethod.TM });
        Checklist loadedTMs = new(possibleTMs.Count);
        StringBuilder log = new($"{allyParty[0].name} TM moves are:");
        //if (possibleTMs.Count > 0) LoadTMs(possibleTMs[0]);
        void LoadTMs(MoveReference moveReference)
        {
            PokeAPI.GetMove(moveReference.move.url, LoadMoveData);

            void LoadMoveData(MoveData moveData)
            {
                PokeAPI.GetTM(moveData, (tm) =>
                {
                    log.Append($"\n{tm.name} => {tm.data.moveData.name}");

                    loadedTMs.FinishStep();
                    if (loadedTMs.isDone)
                    {
                        Logger.Log(log.ToString(), LogFlags.Tests);
                        return;
                    }

                    LoadTMs(possibleTMs[loadedTMs.currentSteps]);
                });
            }
        }

        itemsLoaded.FinishStep();
    }
    #endregion
}
