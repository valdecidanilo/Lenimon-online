using System;
using System.Text;
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

    private MoveDatabase moveDatabase;

    Pokemon[] allyParty;
    Pokemon[] enemyParty;

    private void Awake()
    {
        PokeDatabase.PreloadAssets();
        LoadingScreen.onDoneLoading += Setup;
        moveDatabase = Resources.Load<MoveDatabase>("MoveDatabase");
    }

    private void Setup()
    {
        LoadingScreen.onDoneLoading -= Setup;
        LoadingScreen.onDoneLoading += StartBattle;
        GenerateParties();
    }

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

        alliesLoaded.onProgress += (p) => LoadingScreen.AddOrChangeQueue(alliesLoaded, $"Loading ally party[{alliesLoaded.currentSteps+1}/{alliesLoaded.requiredSteps}]...");
        alliesLoaded.onCompleted += () =>
        {
            //load enemies
            partySize = Random.Range(1, 7);
            partyLevel = Mathf.Min(100, encounterLevel + (6 - partySize));
            enemyParty = new Pokemon[partySize];

            Checklist opponentLoaded = new(partySize);
            opponentLoaded.onProgress += (p) => { if (!requiredEnemy.isDone) requiredEnemy.FinishStep(); };
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
        PokeAPI.GetPokemonData(pokemonId, (data) =>
        {
            Pokemon.GetLoadedPokemon(data, level, onFinished);
        });
    }

    private void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame) SceneManager.LoadScene(0);
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
}
