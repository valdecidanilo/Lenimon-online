using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.OpponentAI
{
    public class WildOpponent : Opponent
    {
        public WildOpponent()
        {
            name = "Wild";
            referenceText = "Wild";
        }

        public override void SetupBag(int partyLevel, System.Action onFinished)
        {
            onFinished?.Invoke(); // Sem itens para coisinhas da grama
        }
        
        public override MoveModel ChooseMove(Pokemon pokemon)
        {
            List<int> options = new(4);
            for (var i = 0; i < options.Capacity; i++)
                if (activePokemon.moves[i] != null && activePokemon.moves[i].pp > 0)
                    options.Add(i);
            var moveModel = options.Count > 0 ? activePokemon.moves[options[Random.Range(0, options.Count)]] : MoveHelper.Struggle();
            return moveModel;
        }

        public override IEnumerator PickPokemon(PickPokemonEvent evt)
        {
            evt.partyId = 0;
            evt.pickedPokemon = party[0];
            yield break;
        }
    }
}