using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class TestScript : MonoBehaviour
{
    [SerializeField] BattleSetup battle;

    private Checklist loaded;
    Pokemon ally = null;
    Pokemon enemy = null;

    private void Awake()
    {
        loaded = new(2);
        loaded.onCompleted += StartBattle;

        PokeAPI.GetPokemonData("eevee", SetEnemy);
        PokeAPI.GetPokemonData("bulbasaur", SetAlly);
    }

    private void SetAlly(PokemonData pokemon)
    {
        Pokemon.GetLoadedPokemon(pokemon, Random.Range(0, 100), (pokemon) =>
        {
            ally = pokemon;
            CheckPokemon(ally);
            loaded.FinishStep();
        });
    }

    private void SetEnemy(PokemonData pokemon)
    {
        Pokemon.GetLoadedPokemon(pokemon, Random.Range(0, 100), (pokemon) =>
        {
            enemy = pokemon;
            CheckPokemon(enemy);
            loaded.FinishStep();
        });
    }

    private void StartBattle()
    {
        battle.SetupBattle(ally, enemy);
    }

    private void CheckPokemon(Pokemon pokemon)
    {
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

        Debug.Log($"{pokemon.name}\n{type.ToString()}\n{abilities.ToString()}");
    }
}
