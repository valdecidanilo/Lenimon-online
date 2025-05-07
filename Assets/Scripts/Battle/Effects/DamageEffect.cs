using System.Collections;
using System.Collections.Generic;
using LenixSO.Logger;
using UnityEngine;
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
            int damage = evt.dmgEvent.GetDamage();
            Pokemon target = evt.dmgEvent.defender;
            target.DamagePokemon(damage);
            Logger.Log($"dealt {damage} damage to {target.name} " +
                       $"({target.battleStats.hp}/{target.stats.hp})", LogFlags.Game);
            //sub effect
            //TODO:
        }
    }
}