using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LenixSO.Logger;
using Logger = LenixSO.Logger.Logger;

namespace Battle
{
    public class DamageEffect : Effect
    {
        private int subEffectChance;
        private Effect subEffect;
        public DamageEffect(Effect subEffect = null, int chance = 100)
        {
            this.subEffect = subEffect;
            subEffectChance = chance;
        }
        
        public override IEnumerator EffectSequence(BattleEvent evt)
        {
            yield return null;
            //check hit
            //damage
            evt.attackEvent.DealDamage();
            //sub effect
            //TODO:
        }
    }
}