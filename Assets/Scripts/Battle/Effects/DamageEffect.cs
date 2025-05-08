using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using LenixSO.Logger;
using Logger = LenixSO.Logger.Logger;

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
            yield return null;
            //damage
            evt.attackEvent.DealDamage();
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