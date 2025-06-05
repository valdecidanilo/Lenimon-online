using System;
using System.Collections;

namespace Battle
{
    public abstract class Effect
    {
        public int subEffectChance = 100;
        public Effect subEffect;
        public Action<BattleEvent> subEffectSetup;
        public abstract IEnumerator EffectSequence(BattleEvent evt);
    }
}