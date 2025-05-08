using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    public class DamageMoveEffect : MoveEffectCreator
    {
        private int subEffectChance;
        private Effect subEffect;
        private Action<BattleEvent> subEffectSetup;

        public DamageMoveEffect(Effect subEffect = null, int chance = 100, Action<BattleEvent> subEffectSetup = null)
        {
            this.subEffect = subEffect;
            subEffectChance = chance;
            this.subEffectSetup = subEffectSetup;
        }
        
        public override void AddEffect(MoveModel move)
        {
            move.effect = new DamageEffect(subEffect, subEffectChance, subEffectSetup);
        }
    }
}