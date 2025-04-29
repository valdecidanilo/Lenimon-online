using System;
using UnityEngine;
using System.Collections.Generic;
using LenixSO.Logger;
using Logger = LenixSO.Logger.Logger;

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
        Logger.Log($"pokemon route: {route}", LogFlags.API);
        WebConnection.GetRequest(route, onSuccess);
    }

    public static void GetSprite(PokemonData pokemon, Action<Sprite> onSuccess, bool backSprite = false)
    {
        GetTexture(pokemon, (txt) =>
        {
            Sprite sprite;
            if (txt != null)
            {
                Rect rect = new(0, 0, txt.width, txt.height);
                Vector2 pivot = Vector2.one / 2f;
                sprite = Sprite.Create(txt, rect, pivot);
            }
            else
            {
                string sufix = backSprite ? "Back" : "Front";
                sprite = Resources.Load<Sprite>($"MissingNo{sufix}");
            }
            onSuccess?.Invoke(sprite);
        }, backSprite);
    }

    public static void GetIcon(PokemonData pokemon, Action<Sprite> onSuccess)
    {
        string route = pokemon.sprites.versions.gen7.icons.front_default;
        bool hasIcon = !string.IsNullOrEmpty(pokemon.sprites.versions.gen7.icons.front_default);
        if (!hasIcon && !string.IsNullOrEmpty(pokemon.sprites.versions.gen8.icons.front_default)) 
            route = pokemon.sprites.versions.gen8.icons.front_default;
        hasIcon = !string.IsNullOrEmpty(route);
        if (!hasIcon)
        {
            Logger.LogError("No icons found", LogFlags.API);
            onSuccess?.Invoke(Resources.Load<Sprite>($"MissingNoIcon"));
            return;
        }

        Logger.Log($"icon route: {route}", LogFlags.API);
        WebConnection.GetTexture(route, (txt) =>
        {
            Rect rect = new(0, 0, txt.width, txt.height);
            Vector2 pivot = Vector2.one / 2f;
            Sprite sprite = Sprite.Create(txt, rect, pivot);
            onSuccess?.Invoke(sprite);
        });
    }

    public static void GetTexture(PokemonData pokemon, Action<Texture2D> onSuccess, bool backSprite = false)
    {
        string route = backSprite ? pokemon.sprites.back_default : pokemon.sprites.front_default;
        if (string.IsNullOrEmpty(route))
        {
            string prefix = backSprite ? "Back" : "Front";
            Logger.LogError($"{prefix} Sprite not found", LogFlags.API);
            onSuccess?.Invoke(null);
            return;
        }
        WebConnection.GetTexture(route, onSuccess);
    }

    public static void GetMove(string name, Action<MoveData> onSuccess)
    {
        string route = $"https://pokeapi.co/api/v2/move/{name}";
        Logger.Log($"move route: {route}", LogFlags.API);
        WebConnection.GetRequest<MoveData>(route, (data) =>
        {
            Logger.Log($"move type route: {data.typeOfMove.url}", LogFlags.API);
            if (moveTypes.ContainsKey(data.typeOfMove.url))
            {
                data.moveTypeData = moveTypes[data.typeOfMove.url];
                onSuccess?.Invoke(data);
            }
            else
            {
                WebConnection.GetRequest<MoveTypeData>(data.typeOfMove.url, (type) =>
                {
                    moveTypes[data.typeOfMove.url] = type;
                    data.moveTypeData = type;
                    onSuccess?.Invoke(data);
                });
            }
        });
    }

    //cashe
    private static Dictionary<string, MoveTypeData> moveTypes = new();
}