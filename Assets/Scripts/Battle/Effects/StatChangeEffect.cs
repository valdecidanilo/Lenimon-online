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
        private readonly List<StatChange> changeRef;

        private readonly int mainSign;

        private readonly List<StatType> mainStats;
        private readonly List<int> mainChanges;
        private readonly string mainChangeText;

        private readonly List<StatType> altStats;
        private readonly List<int> altChanges;
        private readonly string altChangeText;
        
        public StatChangeEffect(List<StatChange> statChanges)
        {
            changeRef = statChanges;

            mainStats = new();
            mainChanges = new();
            altStats = new();
            altChanges = new();

            mainSign = (int)Mathf.Sign(changeRef[0].change);
            StringBuilder mainText = new();
            StringBuilder altText = new();
            for (int i = 0; i < statChanges.Count; i++)
            {
                bool mainChange = (int)Mathf.Sign(changeRef[i].change) == mainSign;
                StatType statType = ApiToStat(changeRef[i].stat);
                int change = changeRef[i].change;
                if (mainChange)
                {
                    //if (mainChanges.Count > 0)
                    //{
                    //    //change "and" for ","
                    //    mainText.Remove(mainText.Length - 5, 5);
                    //    mainText.Append(", ");
                    //}
                    mainText.Append($"{changeRef[i].stat.name.Replace("-", " ")}");
                    mainText.Append($" and ");
                    mainStats.Add(statType);
                    mainChanges.Add(change);
                }
                else
                {
                    //if (altChanges.Count > 0)
                    //{
                    //    //change "and" for ","
                    //    altText.Remove(altText.Length - 5, 5);
                    //    altText.Append(", ");
                    //}
                    altText.Append($"{changeRef[i].stat.name.Replace("-", " ")}");
                    altText.Append($" and ");
                    altStats.Add(statType);
                    altChanges.Add(change);
                }
            }
            mainText.Remove(mainText.Length - 5, 5);
            mainText.Append(mainSign > 0 ? " rose" : " fell");
            mainChangeText = mainText.ToString();
            //Logger.Log(mainChangeText);
            if (altChanges.Count <= 0) return;
            altText.Remove(altText.Length - 5, 5);
            altText.Append(mainSign < 0 ? " rose" : " fell");
            altChangeText = altText.ToString();
            //Logger.Log(altChangeText);
        }
        
        public override IEnumerator EffectSequence(BattleEvent evt)
        {
            StringBuilder sb = new($"{evt.target.name}'s ");
            Stats stats = evt.target.battleStats;
            for (int i = 0; i < mainChanges.Count; i++)
            {
                stats[mainStats[i]] = Mathf.Clamp(stats[mainStats[i]] + mainChanges[i], -6, 6);
            }
            sb.Append($"{mainChangeText}!");
            Logger.Log(sb.ToString(), LogFlags.Game);
            yield return FightMenu.StatusChangeEffect(evt.target, mainSign > 0);
            yield return Announcer.AnnounceCoroutine(sb.ToString(), holdTime: 1.5f);

            if (altChanges.Count <= 0) yield break;

            sb = new($"{evt.target.name}'s ");
            for (int i = 0; i < altChanges.Count; i++)
            {
                stats[altStats[i]] = Mathf.Clamp(stats[altStats[i]] + altChanges[i], -6, 6);
            }
            sb.Append($"{altChangeText}!");
            Logger.Log(sb.ToString(), LogFlags.Game);
            yield return FightMenu.StatusChangeEffect(evt.target, mainSign < 0);
            yield return Announcer.AnnounceCoroutine(sb.ToString(), holdTime: 1.5f);
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