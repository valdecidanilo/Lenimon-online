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
            //damage
            int hits = Random.Range(evt.attackEvent.minHits, evt.attackEvent.maxHits + 1);
            for (int i = 0; i < hits; i++)
            {
                yield return evt.attackEvent.DealDamage();
                if (i < hits - 1) yield return attacksDelay;
            }

            if (evt.attackEvent.maxHits > evt.attackEvent.minHits)
                yield return Announcer.Announce($"It hit {hits} times!", holdTime: 1);

            //sub effect
            //check chance
            if (subEffect == null) yield break;
            int r = Random.Range(1, 101);
            if (r > subEffectChance) yield break;
            subEffectSetup?.Invoke(evt);
            yield return subEffect.EffectSequence(evt);
        }
    }
}