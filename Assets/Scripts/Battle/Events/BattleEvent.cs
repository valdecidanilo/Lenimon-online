using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    public class BattleEvent
    {
        public Trainer user;
        public Pokemon origin;
        public Pokemon target;
        public MoveModel move;
        public AttackEvent attackEvent;
        public bool failed;
    }
}