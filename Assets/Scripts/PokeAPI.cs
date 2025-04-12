using System;
using UnityEngine;

public static class PokeAPI
{
    public static void GetPokemonData(string pokemonName, Action<PokemonData> onSuccess)
    {
        string route = $"https://pokeapi.co/api/v2/pokemon/{pokemonName.ToLower()}";
        WebConnection.GetRequest(route, onSuccess);
    }

    public static void GetPokemonData(int index, Action<PokemonData> onSuccess)
    {
        string route = $"https://pokeapi.co/api/v2/pokemon/{index}";
        WebConnection.GetRequest(route, onSuccess);
    }

    public static void GetSprite(PokemonData pokemon, Action<Sprite> onSuccess, bool backSprite = false)
    {
        GetTexture(pokemon, (txt) =>
        {
            Rect rect = new(0, 0, txt.width, txt.height);
            Vector2 pivot = Vector2.one / 2f;
            Sprite sprite = Sprite.Create(txt, rect, pivot);
            onSuccess?.Invoke(sprite);
        }, backSprite);
    }

    public static void GetTexture(PokemonData pokemon, Action<Texture2D> onSuccess, bool backSprite = false)
    {
        string route = backSprite ? pokemon.sprites.back_default : pokemon.sprites.front_default;
        WebConnection.GetTexture(route, onSuccess);
    }
}