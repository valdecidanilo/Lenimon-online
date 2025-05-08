using LenixSO.Logger;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LenixSO.Logger;
using Logger = LenixSO.Logger.Logger;

namespace Battle
{
    public class DamageEvent
    {
        public Pokemon attacker;
        public Pokemon defender;
        public int power;
        public MoveType typeOfMove;
        public float modifier; //burn attack reduction, screens, weather, flashFire like abilities, critical, double damage effect, STAB, type multiplier 1 and 2

        public DamageEvent(Pokemon attacker, Pokemon defender, MoveModel move)
        {
            this.attacker = attacker;
            this.defender = defender;
            power = move.power ?? 0;
            typeOfMove = move.typeOfMove;
            modifier = 1;
        }

        //https://bulbapedia.bulbagarden.net/wiki/Generation_III
        public void DealDamage()
        {
            Stats attackerStats = CalculateModifiers(attacker);
            Stats defenderStats = CalculateModifiers(defender);
            int atk = typeOfMove switch
            {
                MoveType.Physical => attackerStats.atk,
                MoveType.Special => attackerStats.sAtk,
                _ => 0
            };
            int def = typeOfMove switch
            {
                MoveType.Physical => defenderStats.def,
                MoveType.Special => defenderStats.sDef,
                _ => 0
            };
            int baseDamage = (((((2 * attacker.level) / 5) + 2) * power * (atk / def)) / 50) + 2;
            int finalDamage = Mathf.FloorToInt((baseDamage * modifier));
            //finalDamage = Mathf.FloorToInt(finalDamage * (Random.Range(85, 101) / 100f));

            Logger.Log($"dealt {finalDamage} damage to {defender.name} " +
                       $"({defender.battleStats.hp}/{defender.stats.hp})", LogFlags.Game);
            defender.DamagePokemon(finalDamage);
        }

        private Stats CalculateModifiers(Pokemon pokemon)
        {
            Stats stats = Stats.Copy(pokemon.stats);
            StatType type = StatType.hp;
            int stage;
            int upper;
            int lower;

            //regular stats
            int finalStat = (int)StatType.spd;
            for (int i = (int)StatType.atk; i <= finalStat; i++)
            {
                type = (StatType)i;
                stage = pokemon.battleStats[type];
                upper = 2 + Mathf.Max(stage, 0);
                lower = 2 - Mathf.Min(stage, 0);
                stats[type] = pokemon.stats[type] * (upper / lower);
            }

            //accuracy
            type = StatType.acc;
            stage = pokemon.battleStats[type];
            upper = 3 + Mathf.Max(stage, 0);
            lower = 3 - Mathf.Min(stage, 0);
            stats[type] = pokemon.stats[type] * (upper / lower);

            //evasion
            type = StatType.eva;
            stage = pokemon.battleStats[type];
            upper = 3 - Mathf.Min(stage, 0);
            lower = 3 + Mathf.Max(stage, 0);
            stats[type] = pokemon.stats[type] * (upper / lower);

            return stats;
        }
    }
}