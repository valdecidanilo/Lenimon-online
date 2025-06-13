using LenixSO.Logger;
using System;
using System.Collections;

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
        return base.PickPokemon(evt);
    }
}
