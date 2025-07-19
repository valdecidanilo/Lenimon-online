using System.Collections.Generic;
using UnityEngine;

namespace Battle.OpponentAI
{
    public class FullRandomAI : Opponent
    {
        public override MoveModel ChooseMove(Pokemon pokemon)
        {
            if (activePokemon.battleStats.hp <= activePokemon.stats.hp * .25f &&
                (bag.items.Count > 0 && bag.items[0].amount > 0))
                return UseHealItem();
            List<int> options = new(4);
            for (var i = 0; i < options.Capacity; i++)
                if (activePokemon.moves[i] != null && activePokemon.moves[i].pp > 0)
                    options.Add(i);

            var moveModel = options.Count > 0 ? activePokemon.moves[options[Random.Range(0, options.Count)]] : MoveHelper.Struggle();
            return moveModel;
        }
    }
}
