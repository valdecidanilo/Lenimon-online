using System.Collections.Generic;
using UnityEngine;

public class FullRandomAI : Opponent
{
    public override MoveModel ChooseMove(Pokemon pokemon)
    {
        if (activePokemon.battleStats.hp <= activePokemon.stats.hp * .25f &&
            (bag.items.Count > 0 && bag.items[0].amount > 0))
            return UseHealItem();
        List<int> options = new(4);
        for (int i = 0; i < options.Capacity; i++)
            if (pokemon.moves[i] != null && pokemon.moves[i].pp > 0)
                options.Add(i);

        MoveModel pickedMove;
        if(options.Count > 0) pickedMove = activePokemon.moves[options[Random.Range(0, options.Count)]];
        else pickedMove = MoveHelper.Struggle();
        return pickedMove;
    }
}
