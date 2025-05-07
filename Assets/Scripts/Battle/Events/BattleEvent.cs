using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    public class BattleEvent
    {
        public Pokemon origin;
        public Pokemon target;
        public MoveModel move;
        public DamageEvent dmgEvent;
        public bool failed;
        public List<string> messages;
    }
}