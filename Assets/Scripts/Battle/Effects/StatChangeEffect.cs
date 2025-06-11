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
        public static readonly Dictionary<string, StatType> StatTypes = new()
        {
            { "attack", StatType.atk },
            { "defense", StatType.def },
            { "special-attack", StatType.sAtk },
            { "special-defense", StatType.sDef },
            { "speed", StatType.spd },
            { "accuracy", StatType.acc },
            { "evasion", StatType.eva },
        };

        public static readonly string[] StatNames = new[]
        {
            "attack",
            "defense",
            "special attack",
            "special defense",
            "speed",
            "accuracy",
            "evasion",
        };
        
        private readonly List<StatChange> changeRef;
        private readonly List<StatType> statLookUp;
        private readonly List<int> changeLookUp;

        private readonly int mainSign;
        private readonly List<int> mainChanges;
        private readonly List<int> altChanges;
        
        public StatChangeEffect(List<StatChange> statChanges)
        {
            changeRef = statChanges;

            statLookUp = new(statChanges.Count);
            changeLookUp = new(statChanges.Count);
            
            mainChanges = new();
            altChanges = new();

            mainSign = (int)Mathf.Sign(changeRef[0].change);
            for (int i = 0; i < statChanges.Count; i++)
            {
                string statName = changeRef[i].stat.name;
                StatType statType = StatTypes[statName];
                statLookUp.Add(statType);
                changeLookUp.Add(changeRef[i].change);
                bool mainChange = (int)Mathf.Sign(changeRef[i].change) == mainSign;
                if (mainChange) mainChanges.Add(i);
                else altChanges.Add(i);
            }
        }
        
        public override IEnumerator EffectSequence(BattleEvent evt)
        {
            StringBuilder sb = new($"{evt.target.name}'s ");
            Stats stats = evt.target.battleStats;
            for (int i = 0; i < mainChanges.Count; i++)
            {
                StatType statType = statLookUp[mainChanges[i]];
                int newValue = Mathf.Clamp(stats[statType] + changeLookUp[mainChanges[i]], -6, 6);
                stats[statType] = newValue;
                sb.Append($"{StatNames[(int)statType - 1]}, ");
            }

            sb.Remove(sb.Length - 2, 2);
            if (mainChanges.Count > 1)
            {
                const int mod = 2;
                int lastStat = (int)statLookUp[mainChanges[^1]]-1;
                string lastStatText = StatNames[lastStat];
                sb.Remove(sb.Length - (lastStatText.Length + mod), lastStatText.Length + mod);
                sb.Append($" and {lastStatText}");
            }
            sb.Append($" {(mainSign > 0 ? "rose" : "fell")}!");
            Logger.Log(sb.ToString(), LogFlags.Game);
            yield return FightMenu.StatusChangeEffect(evt.target, mainSign > 0);
            yield return Announcer.AnnounceCoroutine(sb.ToString(), holdTime: 1.5f);

            if (altChanges.Count <= 0) yield break;
            
            sb = new($"{evt.target.name}'s ");
            for (int i = 0; i < altChanges.Count; i++)
            {
                StatType statType = statLookUp[altChanges[i]];
                int newValue = Mathf.Clamp(stats[statType] + changeLookUp[altChanges[i]], -6, 6);
                stats[statType] = newValue;
                sb.Append($"{StatNames[(int)statType - 1]}, ");
            }

            sb.Remove(sb.Length - 2, 2);
            if (altChanges.Count > 1)
            {
                const int mod = 2;
                int lastStat = (int)statLookUp[altChanges[^1]]-1;
                string lastStatText = StatNames[lastStat];
                sb.Remove(sb.Length - (lastStatText.Length + mod), lastStatText.Length + mod);
                sb.Append($" and {lastStatText}");
            }
            sb.Append($" {(mainSign < 0 ? "rose" : "fell")}!");
            Logger.Log(sb.ToString(), LogFlags.Game);
            yield return FightMenu.StatusChangeEffect(evt.target, mainSign < 0);
            yield return Announcer.AnnounceCoroutine(sb.ToString(), holdTime: 1.5f);
        }
    }
}