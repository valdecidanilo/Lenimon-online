using LenixSO.Logger;
using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public abstract class Opponent : Trainer
{
    public virtual void SetupBag(int partyLevel, Action onFinished)
    {
        string healItem = partyLevel switch
        {
            < 18 => "potion",
            < 25 => "super-potion",
            < 30 => "soda-pop",
            < 40 => "lemonade",
            < 50 => "moomoo-milk",
            < 70 => "hyper-potion",
            _ => "max-potion",
        };


        string route = $"{PokeAPI.baseRoute}item/{healItem}";
        PokeAPI.GetItem(route, (item) =>
        {
            item.amount = party.Length;
            bag.items.Add(item);
            item.battleEffect = ItemEffect.GenerateItemEffect(item);
            onFinished?.Invoke();
        });
    }
    public abstract MoveModel ChooseMove(Pokemon pokemon);

    protected virtual MoveModel UseHealItem()
    {
        ItemModel item = bag.items[0];
        item.amount--;
        if (item.amount <= 0) bag.items.Remove(item);
        MoveModel mockMove = ItemEffect.CreateMockMove(item);
        mockMove.priority--;
        return mockMove;
    }

    public override IEnumerator PickPokemon(PickPokemonEvent evt)
    {
        List<int> possiblePokemons = new(party.Length);
        for (int i = 0; i < party.Length; i++)
        {
            if(party[i].fainted || party[i] == activePokemon) continue;
            possiblePokemons.Add(i);
        }

        if (possiblePokemons.Count > 0)
        {
            evt.partyId = possiblePokemons[Random.Range(0, possiblePokemons.Count)];
            evt.pickedPokemon = party[evt.partyId];
        }
        else evt.noValidPokemon = true;
        yield break;
    }
}
