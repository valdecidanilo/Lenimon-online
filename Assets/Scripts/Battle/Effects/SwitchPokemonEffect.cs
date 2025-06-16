using System.Collections;

namespace Battle
{
    /// <summary>
    /// New rules for PickPokemonEvent on the switch effect:
    /// "canLearnTM" means it doesn't have a pokemon to switch to
    /// </summary>
    public class SwitchPokemonEffect : Effect
    {
        public int newPokemon = -1;
        
        public override IEnumerator EffectSequence(BattleEvent evt)
        {
            while (newPokemon < 0)
            {
                PickPokemonEvent pokemonEvent = new(false);
                yield return evt.targetTrainer.PickPokemon(pokemonEvent);
                if(pokemonEvent.canLearnTM) break;
                if (pokemonEvent.pickedPokemon == null) continue;
                if (!pokemonEvent.pickedPokemon.fainted)
                {
                    if (pokemonEvent.pickedPokemon != evt.target)
                        newPokemon = pokemonEvent.partyId;
                    else
                        yield return Announcer.AnnounceCoroutine("Pokemon is already in battle!", true, 
                            onDone: Announcer.CloseAnnouncement);
                }
                else
                    yield return Announcer.AnnounceCoroutine("You can't switch to a fainted pokemon!", true, 
                        onDone: Announcer.CloseAnnouncement);
            }

            if (newPokemon >= 0)
            {
                PartyMenu.ClosePartyMenu();
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