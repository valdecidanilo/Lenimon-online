using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullRandomAI : Opponent
{
    public override MoveModel ChooseMove(Pokemon pokemon)
    {
        if (activePokemon.battleStats.hp <= activePokemon.stats.hp * .25f &&
            (bag.items.Count > 0 && bag.items[0].amount > 0))
            return UseHealItem();
        MoveModel pickedMove = activePokemon.moves[Random.Range(0, 4)];
        while (pickedMove == null) pickedMove = activePokemon.moves[Random.Range(0, 4)];
        return pickedMove;
    }

    private MoveModel UseHealItem()
    {
        ItemModel item = bag.items[0];
        item.amount--;
        if(item.amount <= 0) bag.items.Remove(item);
        MoveModel mockMove = ItemEffect.CreateMockMove(item);
        mockMove.priority--;
        return mockMove;
    }
}
