using System.Collections.Generic;
using UnityEngine;

namespace Battle.OpponentAI
{
    public class MockRemotePlayerAI : Opponent
    {
        private static readonly string[] NamesTest =
        {
            "JuniorAI",
            "ReiAI",
            "AlineAI",
            "CarlosAI",
            "LeiteAI",
            "LoboAI",
            "SabrinaAI",
        };

        public MockRemotePlayerAI()
        {
            
            name = NamesTest[Random.Range(0, NamesTest.Length)];
            referenceText = "Opponent's";
        }

        public override MoveModel ChooseMove(Pokemon pokemon)
        {
            List<int> options = new(4);
            for (int i = 0; i < pokemon.moves.Length; i++)
            {
                if (pokemon.moves[i] != null && pokemon.moves[i].pp > 0)
                    options.Add(i);
            }

            return options.Count > 0 ? pokemon.moves[options[Random.Range(0, options.Count)]] : MoveHelper.Struggle();
        }

        public override void SetupBag(int partyLevel, System.Action onFinished)
        {
            bag.items.Clear();
            base.SetupBag(partyLevel, onFinished);
        }
    }
}