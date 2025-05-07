using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    public class CreatorDamageEffect : EffectCreation
    {
        private int subEffectChance;
        private Effect subEffect;

        public CreatorDamageEffect(Effect subEffect = null, int chance = 100)
        {
            this.subEffect = subEffect;
            subEffectChance = chance;
        }
        
        public override void AddEffect(MoveModel move)
        {
            move.effect = new DamageEffect(subEffect, subEffectChance);
        }
    }
}