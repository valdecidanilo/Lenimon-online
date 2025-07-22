using UnityEngine;

namespace Utils
{
    public class PokeMath
    {
        public static int GainExperience(int opponentLevel, int baseAllyExperience, int experience, int experienceMax, bool isTrainerPokemon = false, bool hasLuckyEgg = false)
        {
            var modifier = 1.0f;
            if (isTrainerPokemon) modifier *= 1.5f;
            if (hasLuckyEgg) modifier *= 1.5f;

            var expGained = Mathf.FloorToInt((baseAllyExperience * opponentLevel * modifier) / 7);

            experience += expGained;
            if (experience >= experienceMax)
            {
                //LevelUp();
            }
            return expGained;
        }
        private static void LevelUp(int currentLevel, int levelMax, int levelsGained = 1, string natureIncreasedStat = "", string natureDecreasedStat = "")
        {
            currentLevel += levelsGained;

            if (currentLevel > levelMax)
                currentLevel = levelMax;
            //stats.hpStats.currentStat = CalculateHp(stats.hpStats.baseStat, stats.hpStats.iv, stats.hpStats.effort, currentLevel);
            //RecalculateStats(stats, currentLevel, natureIncreasedStat, natureDecreasedStat);
        }
        public static int BasicStatCalculation(int baseStat, int iv, int ev, int level)
        {
            return Mathf.Max(0, (((2 * baseStat) + iv + (ev / 4)) * level) / 100);
        }

        public static int NonHpCalculation(Pokemon pokemon, StatType type)
        {
            return Mathf.FloorToInt((
                BasicStatCalculation(
                    pokemon.data.stats[(int)type].base_stat, 
                    pokemon.iv[type], 
                    pokemon.ev[type], 
                    pokemon.level) + 5) * (pokemon.nature[type] / 100f));
        }
    }
}