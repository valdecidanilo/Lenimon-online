using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokeAPI : MonoBehaviour
{
    public void GetPokemon(string pokemonName, Action<Pokemon> onSuccess)
    {
        string route = $"https://pokeapi.co/api/v2/pokemon/{pokemonName.ToLower()}";
        WebConnection.GetRequest(route, onSuccess);
    }

    public void GetPokemon(int index, Action<Pokemon> onSuccess)
    {
        string route = $"https://pokeapi.co/api/v2/pokemon/{index}";
        WebConnection.GetRequest(route, onSuccess);
    }

    public void GetSprite(Pokemon pokemon, Action<Sprite> onSuccess, bool backSprite = false)
    {
        GetTexture(pokemon, (txt) =>
        {
            Rect rect = new(0, 0, txt.width, txt.height);
            Vector2 pivot = Vector2.one / 2f;
            Sprite sprite = Sprite.Create(txt, rect, pivot);
            onSuccess?.Invoke(sprite);
        }, backSprite);
    }

    public void GetTexture(Pokemon pokemon, Action<Texture2D> onSuccess, bool backSprite = false)
    {
        string route = backSprite ? pokemon.sprites.back_default : pokemon.sprites.front_default;
        WebConnection.GetTexture(route, onSuccess);
    }
}
