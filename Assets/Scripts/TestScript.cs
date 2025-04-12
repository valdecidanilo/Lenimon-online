using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class TestScript : MonoBehaviour
{
    [SerializeField] PokeAPI pokeAPI;
    [SerializeField] Image enemy;
    [SerializeField] Image ally;

    private void Awake()
    {
        pokeAPI.GetPokemon("rayquaza", SetEnemy);
        pokeAPI.GetPokemon("rayquaza", SetAlly);
    }

    private void SetAlly(Pokemon pokemon)
    {
        pokeAPI.GetSprite(pokemon, (sprite) => ally.sprite = sprite, true);
    }

    private void SetEnemy(Pokemon pokemon)
    {
        pokeAPI.GetSprite(pokemon, (sprite) => enemy.sprite = sprite);
    }

    private void CheckPokemon(Pokemon pokemon)
    {
        StringBuilder type = new();
        for (int i = 0; i < pokemon.types.Count; i++)
        {
            type.Append(pokemon.types[i].type.name);
            if (i < pokemon.types.Count - 1) type.Append("\\");
        }

        StringBuilder abilities = new();
        for (int i = 0; i < pokemon.abilities.Count; i++)
        {
            Ability ability = pokemon.abilities[i];
            abilities.Append($"{ability.reference.name}");
            if (ability.hidden) abilities.Append("(H)");
            if (i < pokemon.abilities.Count - 1) abilities.Append(" | ");
        }

        Debug.Log($"{pokemon.name}\n{type.ToString()}\n{abilities.ToString()}");
    }
}
