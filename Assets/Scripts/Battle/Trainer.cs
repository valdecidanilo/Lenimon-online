using System;
using System.Collections;

namespace Battle
{
    public class Trainer
    {
        public string name;//you | opponent
        public string referenceText;//you => allied | opponent => opponent's
        public Bag bag = new();
        public Pokemon[] party;
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
