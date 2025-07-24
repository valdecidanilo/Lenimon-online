using System;
using System.Collections;
using System.Collections.Generic;

namespace Battle
{
    public class Trainer
    {
        public string name = "Trainer";//you | opponent
        public string referenceText;//you => allied | opponent => opponent's
        public Bag bag = new();
        public List<Pokemon> party = new();
        public Pokemon activePokemon;

        public MoveModel pickedMove;

        /// <summary>
        /// "isCurrent" will be treated as can't pick currentPokemon
        /// </summary>
        public virtual IEnumerator PickPokemon(PickPokemonEvent evt)
        {
            yield return PartyMenu.PickPokemon(evt);
            PartyMenu.ClosePartyMenu();
        }
        public virtual void SetupBag(int partyLevel, Action onFinished)
        {
            
        }
    }
}
