using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TestScript : MonoBehaviour
{
    [SerializeField] BattleSetup battle;
    [SerializeField] private GameObject loadingScreen;

    MoveDatabase moveDatabase;
    private Checklist loaded;
    Pokemon ally = null;
    Pokemon enemy = null;

    private void Awake()
    {
        loaded = new(2);
        loaded.onCompleted += StartBattle;
        moveDatabase = Resources.Load<MoveDatabase>("MoveDatabase");

        PokeAPI.GetPokemonData(Random.Range(1, 152), SetEnemy);
        PokeAPI.GetPokemonData(Random.Range(1, 152), SetAlly);
    }

    private void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame) SceneManager.LoadScene(0);
        
    }

    private void SetAlly(PokemonData pokemon)
    {
        Pokemon.GetLoadedPokemon(pokemon, Random.Range(1, 100), (pokemon) =>
        {
            ally = pokemon;
            CheckPokemon(ally);
            loaded.FinishStep();
        });
    }

    private void SetEnemy(PokemonData pokemon)
    {
        Pokemon.GetLoadedPokemon(pokemon, Random.Range(1, 100), (pokemon) =>
        {
            enemy = pokemon;
            CheckPokemon(enemy);
            loaded.FinishStep();
        });
    }

    private void StartBattle()
    {
        loadingScreen.SetActive(false);
        battle.SetupBattle(ally, enemy);
        moveDatabase.SetDirty();
    }

    private void CheckPokemon(Pokemon pokemon)
    {
        return;
        StringBuilder type = new();
        for (int i = 0; i < pokemon.data.types.Count; i++)
        {
            type.Append(pokemon.data.types[i].type.name);
            if (i < pokemon.data.types.Count - 1) type.Append("\\");
        }

        StringBuilder abilities = new();
        for (int i = 0; i < pokemon.data.abilities.Count; i++)
        {
            Ability ability = pokemon.data.abilities[i];
            abilities.Append($"{ability.reference.name}");
            if (ability.hidden) abilities.Append("(H)");
            if (i < pokemon.data.abilities.Count - 1) abilities.Append(" | ");
        }

        StringBuilder sprites = new();
        sprites.Append($"gen1: {pokemon.data.sprites.versions.gen1.redblue.front_default != null}\n");
        sprites.Append($"gen2: {pokemon.data.sprites.versions.gen2.gold.front_default != null}\n");
        sprites.Append($"gen3: {pokemon.data.sprites.versions.gen3.rubysapphire.front_default != null}\n");
        sprites.Append($"gen4: {pokemon.data.sprites.versions.gen4.diamondpearl.front_default != null}\n");
        sprites.Append($"gen5: {pokemon.data.sprites.versions.gen5.blackwhite.front_default != null}\n");
        sprites.Append($"gen6: {pokemon.data.sprites.versions.gen6.xy.front_default != null}\n");
        sprites.Append($"gen7: {pokemon.data.sprites.versions.gen7.ultrasunultramoon.front_default != null}\n");
        sprites.Append($"gen8: {pokemon.data.sprites.versions.gen8.icons.front_default != null}\n");

        Debug.Log($"{pokemon.name}\n{type.ToString()}\n{abilities.ToString()}\n\n{sprites.ToString()}");
    }
}
