using System.Collections;

namespace Battle
{
    public class SwitchPokemonEffect : Effect
    {
        public Pokemon newPokemon;

        public SwitchPokemonEffect(Pokemon chosenNewPokemon = null)
        {
            
        }
        
        public override IEnumerator EffectSequence(BattleEvent evt)
        {
            yield return FightMenu.ChangePokemon(evt.origin, evt.target);
        }
    }
}