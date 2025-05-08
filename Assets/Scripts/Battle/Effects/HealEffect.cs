using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LenixSO.Logger;
using Logger = LenixSO.Logger.Logger;

namespace Battle
{
    public class HealEffect : Effect
    {
        private int healAmount;
        private HealType healType;

        public HealEffect(int healAmount, HealType healType = HealType.Raw)
        {
            this.healAmount = healAmount;
            this.healType = healType;
        }

        public override IEnumerator EffectSequence(BattleEvent evt)
        {
            yield return null;
            int heal = healType switch
            {
                HealType.Hp => Mathf.FloorToInt(evt.target.stats.hp * (healAmount / 100f)),
                HealType.Drain => Mathf.FloorToInt(evt.attackEvent.damageDealt * (healAmount / 100f)),
                _ => healAmount,
            };
            evt.target.HealPokemon(heal);
            Logger.Log($"{evt.target.name} healed by {heal} ({evt.target.battleStats.hp}/{evt.target.stats.hp})", LogFlags.Game);
        }

        public enum HealType
        {
            Raw,//raw value
            Hp,//percentage of hp
            Drain//percentage of damage dealt
        }
    }
}