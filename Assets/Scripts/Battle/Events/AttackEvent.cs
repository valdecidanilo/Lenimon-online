using LenixSO.Logger;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LenixSO.Logger;
using Logger = LenixSO.Logger.Logger;

namespace Battle
{
    public class AttackEvent
    {
        private const float STAB = 1.5f;
        
        public Pokemon attacker;
        public Pokemon defender;
        public int power;
        public MoveModel move;
        public MoveType typeOfMove;
        public float modifier; //burn attack reduction, screens, weather, flashFire like abilities, critical, double damage effect, STAB, type multiplier 1 and 2

        public int minHits, maxHits;

        public int damageDealt { get; private set; } = -1;
        private readonly Stats attackerStats;
        private readonly Stats defenderStats;

        public AttackEvent(Pokemon attacker, Pokemon defender, MoveModel move)
        {
            this.attacker = attacker;
            this.defender = defender;
            power = move.power ?? 0;
            this.move = move;
            typeOfMove = move.typeOfMove;
            modifier = 1;

            minHits = move.Data.meta.min_hits ?? 1;
            maxHits = move.Data.meta.max_hits ?? minHits;

            attackerStats = CalculateModifiers(attacker);
            defenderStats = CalculateModifiers(defender);
        }

        public bool CheckHit(out bool missed)
        {
            missed = false;
            return true;
        }

        //https://bulbapedia.bulbagarden.net/wiki/Generation_III
        public IEnumerator DealDamage()
        {
            int atk = typeOfMove switch
            {
                MoveType.Physical => attackerStats.atk,
                MoveType.Special => attackerStats.sAtk,
                _ => 0
            };
            float def = typeOfMove switch
            {
                MoveType.Physical => defenderStats.def,
                MoveType.Special => defenderStats.sDef,
                _ => 0
            };
            float baseDamage = (((((2 * attacker.level) / 5f) + 2) * power * (atk / def)) / 50f) + 2;
            int finalDamage = Mathf.FloorToInt((baseDamage * modifier));
            finalDamage = Mathf.FloorToInt(finalDamage * (Random.Range(85, 101) / 100f));//random modifier

            Logger.Log($"dealt {finalDamage} damage to {defender.name} " +
                       $"({defender.battleStats.hp}/{defender.stats.hp})" +
                       $"\n{atk}({attacker.battleStats[StatType.atk]}) vs {def}", LogFlags.Game);
            damageDealt = finalDamage;
            yield return defender.DamagePokemon(finalDamage);
        }

        private Stats CalculateModifiers(Pokemon pokemon)
        {
            Stats stats = Stats.Copy(pokemon.stats);
            StatType type;
            int stage;
            int upper;
            float lower;

            //regular stats
            int finalStat = (int)StatType.spd;
            for (int i = (int)StatType.atk; i <= finalStat; i++)
            {
                type = (StatType)i;
                stage = pokemon.battleStats[type];
                upper = 2 + Mathf.Max(stage, 0);
                lower = 2 - Mathf.Min(stage, 0);
                stats[type] = Mathf.FloorToInt(stats[type] * (upper / lower));
            }

            //accuracy
            type = StatType.acc;
            stage = pokemon.battleStats[type];
            upper = 3 + Mathf.Max(stage, 0);
            lower = 3 - Mathf.Min(stage, 0);
            stats[type] = Mathf.FloorToInt(stats[type] * (upper / lower));

            //evasion
            type = StatType.eva;
            stage = pokemon.battleStats[type];
            upper = 3 - Mathf.Min(stage, 0);
            lower = 3 + Mathf.Max(stage, 0);
            stats[type] = Mathf.FloorToInt(stats[type] * (upper / lower));

            return stats;
        }
    }
}