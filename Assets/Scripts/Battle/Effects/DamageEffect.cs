using System;
using System.Collections;
using Random = UnityEngine.Random;
using LenixSO.Logger;
using Logger = LenixSO.Logger.Logger;
using UnityEngine;

namespace Battle
{
    public class DamageEffect : Effect
    {
        private int subEffectChance;
        private Effect subEffect;
        private Action<BattleEvent> subEffectSetup;

        public DamageEffect(Effect subEffect = null, int chance = 100, Action<BattleEvent> subEffectSetup = null)
        {
            this.subEffect = subEffect;
            subEffectChance = chance;
            this.subEffectSetup = subEffectSetup;
        }

        public override IEnumerator EffectSequence(BattleEvent evt)
        {
            WaitForSeconds attacksDelay = new(.4f);
            
            //add type modifiers
            Pokemon attacker = evt.attackEvent.attacker;
            Pokemon defender = evt.attackEvent.defender;
            MoveModel move = evt.attackEvent.move;
            float typeMod = 1;
            //type matchup
            string typeEffectMessage = string.Empty;
            for (int i = 0; i < defender.types.Length; i++)
                typeMod *= move.moveType.GetMultiplier(defender.types[i]);
            
            //attack type effectiveness text
            if (typeMod > 1) typeEffectMessage = "It's super effective!";
            else if (typeMod < 1 && typeMod > 0) typeEffectMessage = "It's not very effective...";
            else typeEffectMessage = $"It doesn't affect {evt.user} {defender.name}...";
            
            //STAB
            for (int i = 0; i < attacker.types.Length; i++)
            {
                if(attacker.types[i] != move.moveType) continue;
                typeMod *= 1.5f;
                break;
            }

            evt.attackEvent.modifier *= typeMod;

            //damage
            int hits = Random.Range(evt.attackEvent.minHits, evt.attackEvent.maxHits + 1);
            for (int i = 0; i < hits; i++)
            {
                yield return evt.attackEvent.DealDamage();
                if (i < hits - 1) yield return attacksDelay;
            }

            if (evt.attackEvent.damageDealt > 0 && evt.attackEvent.maxHits > evt.attackEvent.minHits)
                yield return Announcer.Announce($"It hit {hits} times!", holdTime: 1);
            
            if(!string.IsNullOrEmpty(typeEffectMessage))
                yield return Announcer.Announce(typeEffectMessage, holdTime: 1);

            //sub effect
            //check chance
            if(evt.attackEvent.damageDealt <= 0) yield break;
            if (subEffect == null) yield break;
            int r = Random.Range(1, 101);
            if (r > subEffectChance) yield break;
            subEffectSetup?.Invoke(evt);
            yield return subEffect.EffectSequence(evt);
        }
    }
}