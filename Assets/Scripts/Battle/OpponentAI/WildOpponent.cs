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
            for (var i = 0; i < pokemon.moves.Length; i++)
            {
                if (pokemon.moves[i] != null && pokemon.moves[i].pp > 0)
                    options.Add(i);
            }

            MoveModel pickedMove;
            if (options.Count > 0)
                pickedMove = pokemon.moves[options[Random.Range(0, options.Count)]];
            else
                pickedMove = MoveHelper.Struggle();

            return pickedMove;
        }

        public override IEnumerator PickPokemon(PickPokemonEvent evt)
        {
            evt.partyId = 0;
            evt.pickedPokemon = party[0];
            yield break;
        }
    }
}