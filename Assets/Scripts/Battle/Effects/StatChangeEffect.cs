using System.Collections;
using System.Collections.Generic;
using System.Text;
using LenixSO.Logger;
using UnityEngine;
using Logger = LenixSO.Logger.Logger;

namespace Battle
{
    public class StatChangeEffect : Effect
    {
        private List<StatChange> changeRef;
        private readonly StatType[] types;
        private readonly int[] changes;
        
        public StatChangeEffect(List<StatChange> statChanges)
        {
            changeRef = statChanges;
            types = new StatType[statChanges.Count];
            changes = new int[statChanges.Count];
            for (int i = 0; i < types.Length; i++)
            {
                types[i] = ApiToStat(statChanges[i].stat);
                changes[i] = statChanges[i].change;
            }
        }
        
        public override IEnumerator EffectSequence(BattleEvent evt)
        {
            yield return null;
            StringBuilder sb = new($"{evt.target.name}'s ");
            string changeType = changes[0] > 0 ? "increased" : "decreased";
            Stats stats = evt.target.battleStats;
            for (int i = 0; i < changes.Length; i++)
            {
                stats[types[i]] += changes[i];
                sb.Append($"{changeRef[i].stat.name.Replace("-", " ")}");
                if (i < changes.Length - 1) sb.Append(" and ");
            }
            sb.Append($" {changeType}");
            Logger.Log(sb.ToString(), LogFlags.Game);
            evt.target.battleStats = stats;
        }

        private static StatType ApiToStat(ApiReference reference)
        {
            StatType type = reference.name switch
            {
                "attack" => StatType.atk,
                "defense" => StatType.def,
                "special-attack" => StatType.sAtk,
                "special-defense" => StatType.sDef,
                "speed" => StatType.spd,
                "accuracy" => StatType.acc,
                "evasion" => StatType.eva,
                _ => (StatType)999
            };
            return type;
        }
    }
}