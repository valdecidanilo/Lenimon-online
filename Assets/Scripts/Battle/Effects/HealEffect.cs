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
            int heal = healType switch
            {
                HealType.Hp => Mathf.FloorToInt(evt.target.stats.hp * (healAmount / 100f)),
                HealType.Drain => Mathf.FloorToInt(evt.attackEvent.damageDealt * (healAmount / 100f)),
                _ => healAmount,
            };
            int healedHp = evt.target.battleStats.hp;
            yield return evt.target.HealPokemon(heal);
            healedHp = evt.target.battleStats.hp - healedHp;
            switch (healType)
            {
                case HealType.Raw:
                    yield return Announcer.AnnounceCoroutine($"{evt.target.name} recovered {healedHp} hit point{(healedHp > 1 ? "s" : "")}.", holdTime: 2f);
                    break;
                case HealType.Hp:
                    yield return Announcer.AnnounceCoroutine($"{evt.target.name} recovered Hp.", holdTime: 2f);
                    break;
                case HealType.Drain:
                    yield return Announcer.AnnounceCoroutine($"{evt.attackEvent.defender.name} got its enegy drained.", holdTime: 2f);
                    break;
            }
            Logger.Log($"{evt.target.name} healed by {heal} ({evt.target.battleStats.hp}/{evt.target.stats.hp})", LogFlags.Game);
        }

        public enum HealType
        {
            /// <summary>
            /// raw value
            /// </summary>
            Raw,
            /// <summary>
            /// percentage of hp
            /// </summary>
            Hp,
            /// <summary>
            /// percentage of damage dealt
            /// </summary>
            Drain
        }
    }
}