using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    public class DamageEffect : Effect
    {
        public override IEnumerator EffectSequence(BattleEvent evt)
        {
            yield return null;
        }
    }
}