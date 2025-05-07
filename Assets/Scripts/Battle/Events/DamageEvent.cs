using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    public class DamageEvent
    {
        public Pokemon attacker;
        public Pokemon defender;
        public int power;
        public MoveType typeOfMove;
        public float modifier1; //burn attack reduction, screens, weather, flashFire like abilities
        public float modifier2; //critical, double damage effect, STAB, type multiplier 1 and 2

        public DamageEvent(Pokemon attacker, Pokemon defender, MoveModel move)
        {
            this.attacker = attacker;
            this.defender = defender;
            power = move.power;
            typeOfMove = move.typeOfMove;
            modifier1 = 1;
            modifier2 = 1;
        }

        //https://bulbapedia.bulbagarden.net/wiki/Generation_III
        public int GetDamage()
        {
            int atk = typeOfMove switch
            {
                MoveType.Physical => attacker.battleStats.atk,
                MoveType.Special => attacker.battleStats.sAtk,
                _ => 0
            };
            int def = typeOfMove switch
            {
                MoveType.Physical => defender.battleStats.def,
                MoveType.Special => defender.battleStats.sDef,
                _ => 0
            };
            int baseDamage = (((2 * attacker.level / 5) + 2) * power * (atk / def)) / 50;
            int finalDamage = Mathf.FloorToInt((baseDamage * modifier1) + 2);
            finalDamage = Mathf.FloorToInt(finalDamage * modifier2 * (Random.Range(85, 101) / 100f));
            return finalDamage;
        }
    }
}