using System;
using UnityEngine;
using System.Collections.Generic;
using AddressableAsyncInstances;
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
            if (txt != null)
            {
                Rect rect = new(0, 0, txt.width, txt.height);
                Vector2 pivot = Vector2.one / 2f;
                Sprite sprite = Sprite.Create(txt, rect, pivot);
                onSuccess?.Invoke(sprite);
                return;
            }

            string sufix = backSprite ? "Back" : "Front";
            AAAsset<Sprite>.LoadAsset($"MissingNo{sufix}", onSuccess);

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
            AAAsset<Sprite>.LoadAsset("MissingNoIcon", (sprite) =>
            {
                Logger.LogError("No icons found", LogFlags.API);
                onSuccess?.Invoke(sprite);
            });
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

    public static void GetMove(string route, Action<MoveData> onSuccess)
    {
        Logger.Log($"move route: {route}", LogFlags.API);
        if (PokeDatabase.moves.TryGetValue(route, out var move)) SetMoveType(move);
        else WebConnection.GetRequest<MoveData>(route, SetMoveType);

        void SetMoveType(MoveData move)
        {
            PokeDatabase.moves[route] = move;
            Logger.Log($"move type route: {move.typeOfMove.url}", LogFlags.API);
            if (PokeDatabase.moveTypes.TryGetValue(move.typeOfMove.url, out var moveType)) ReturnMove(moveType);
            else WebConnection.GetRequest<MoveTypeData>(move.typeOfMove.url, ReturnMove);

            void ReturnMove(MoveTypeData moveType)
            {
                PokeDatabase.moveTypes[move.typeOfMove.url] = moveType;
                move.moveTypeData = moveType;
                onSuccess?.Invoke(move);
            }
        }
    }

    public static void GetAbility(string route, Action<AbilityData> onSuccess)
    {
        if (PokeDatabase.abilities.TryGetValue(route, out var ability)) ReturnAbility(ability);
        else WebConnection.GetRequest<AbilityData>(route, ReturnAbility);

        void ReturnAbility(AbilityData ability)
        {
            PokeDatabase.abilities[route] = ability;
            onSuccess?.Invoke(ability);
        }
    }
    
    //helpers
    public static string SmallestFlavorText(List<FlavorText> entries, string language = "en")
    {
        string text = null;
        for (int i = 0; i < entries.Count; i++)
        {
            FlavorText flavorText = entries[i];
            if(flavorText.language.name == language)
            {
                if(string.IsNullOrEmpty(text) || flavorText.text.Length < text.Length)
                    text = flavorText.text;
            }
        }

        return text;
    }
}