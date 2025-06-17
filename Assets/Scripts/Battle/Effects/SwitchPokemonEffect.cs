using System.Collections;

namespace Battle
{
    public class SwitchPokemonEffect : Effect
    {
        public int newPokemon = -1;
        
        public override IEnumerator EffectSequence(BattleEvent evt)
        {
            while (newPokemon < 0)
            {
                PickPokemonEvent pokemonEvent = new(fainted: false, current: false);
                yield return evt.targetTrainer.PickPokemon(pokemonEvent);
                if (!pokemonEvent.noValidPokemon)
                {
                    if (!(pokemonEvent.partyId < 0)) //not canceled
                        newPokemon = pokemonEvent.partyId;
                }
                else break;
            }

            if (newPokemon >= 0)
            {
                FightMenu.EnableFightAnnouncer();
                yield return SwitchPokemonMessage(evt);
                yield return FightMenu.ChangePokemon(evt.targetTrainer, newPokemon);
                newPokemon = -1;
            }
            else evt.failed = true;
        }
        

        public static IEnumerator SwitchPokemonMessage(BattleEvent evt)
        {
            yield return Announcer.AnnounceCoroutine($"{evt.targetTrainer.name} withdrew {evt.target.name}.", holdTime: .6f);
        }
    }
}