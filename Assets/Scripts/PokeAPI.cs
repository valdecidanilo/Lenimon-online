using System;
using UnityEngine;
using System.Collections.Generic;
using AddressableAsyncInstances;
using LenixSO.Logger;
using Logger = LenixSO.Logger.Logger;

public static class PokeAPI
{
    public const string baseRoute = "https://pokeapi.co/api/v2/";
    public const string struggleMoveRoute = "https://pokeapi.co/api/v2/move/struggle";

    public static void GetPokemonData(string pokemonName, Action<PokemonData> onSuccess)
    {
        string route = $"{baseRoute}/pokemon/{pokemonName.ToLower()}";
        WebConnection.GetRequest(route, onSuccess);
    }

    public static void GetPokemonData(int index, Action<PokemonData> onSuccess)
    {
        string route = $"{baseRoute}/pokemon/{index}";
        Logger.Log($"pokemon route: {route}", LogFlags.API);
        WebConnection.GetRequest(route, onSuccess);
    }

    public static void GetPokemonSprite(PokemonData pokemon, Action<Sprite> onSuccess, bool backSprite = false)
    {
        GetPokemonTexture(pokemon, (txt) =>
        {
            if (txt != null)
            {
                onSuccess?.Invoke(GenerateSprite(txt));
                return;
            };
            onSuccess?.Invoke(null);

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
            onSuccess?.Invoke(null);
            return;
        }

        Logger.Log($"icon route: {route}", LogFlags.API);
        WebConnection.GetTexture(route, (txt) =>
        {
            onSuccess?.Invoke(GenerateSprite(txt));
        });
    }

    public static void GetPokemonTexture(PokemonData pokemon, Action<Texture2D> onSuccess, bool backSprite = false)
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

    public static void GetMoveData(string route, Action<MoveData> onSuccess)
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
    
    public static void GetItem(string route, Action<ItemModel> onSuccess)
    {
        GetItemData(route, ReturnItem);

        void ReturnItem(ItemData data)
        {
            PokeDatabase.items[route] = data;
            ItemModel item = new(data);
            item.sprite = data.sprite;
            onSuccess?.Invoke(item);
        }
    }

    public static void GetItemData(string route, Action<ItemData> onSuccess)
    {
        if (PokeDatabase.items.TryGetValue(route, out var item)) ReturnData(item);
        else WebConnection.GetRequest<ItemData>(route, ReturnData);

        void ReturnData(ItemData data)
        {
            PokeDatabase.items[route] = data;
            if (data.sprite == null && data.spriteRoute != null)
            {
                if (string.IsNullOrEmpty(data.spriteRoute.defaultIcon))
                {
                    data.sprite = PokeDatabase.genericIcon;
                    Logger.LogError($"{data.name} icon not found", LogFlags.API);
                    onSuccess?.Invoke(data);
                    return;
                }
                WebConnection.GetTexture(data.spriteRoute.defaultIcon, (txt) =>
                {
                    data.sprite = GenerateSprite(txt);
                    onSuccess?.Invoke(data);
                });
            }
            else onSuccess?.Invoke(data);
        }
    }

    public static void GetTM(MoveData move, Action<TMModel> onSuccess)
    {
        if (move.machines is { Count: <= 0 }) return;
        string route = move.machines[0].machine.url;
        if (PokeDatabase.TMs.TryGetValue(move, out var tm)) ReturnTM(tm);
        else WebConnection.GetRequest<TMData>(route, ReturnTM);

        void ReturnTM(TMData data)
        {
            PokeDatabase.TMs[move] = data;
            data.moveData = move;
            //get item data
            GetItemData(data.item.url, (item) =>
            {
                data.itemData = item;
                if (data.icon == null || data.icon == PokeDatabase.genericIcon) data.itemData.sprite = PokeDatabase.genericTM;
                TMModel tm = new(data);
                onSuccess?.Invoke(tm);
            });
        }
    }

    //helpers
    private static Sprite GenerateSprite(Texture2D txt)
    {
        Rect rect = new(0, 0, txt.width, txt.height);
        Vector2 pivot = Vector2.one / 2f;
        return Sprite.Create(txt, rect, pivot);
    }
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

        return text ?? string.Empty;
    }

    public static string SmallestFlavorText(List<ItemFlavorText> entries, string language = "en")
    {
        string text = null;
        for (int i = 0; i < entries.Count; i++)
        {
            ItemFlavorText flavorText = entries[i];
            if (flavorText.language.name == language)
            {
                if (string.IsNullOrEmpty(text) || flavorText.text.Length < text.Length)
                    text = flavorText.text;
            }
        }

        return text ?? string.Empty;
    }
}