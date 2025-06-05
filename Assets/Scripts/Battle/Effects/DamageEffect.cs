using System;
using System.Collections;
using Random = UnityEngine.Random;
using LenixSO.Logger;
using Logger = LenixSO.Logger.Logger;
using UnityEngine;

namespace Battle
{
    public class DamageEffect : Effect
    {
        public override IEnumerator EffectSequence(BattleEvent evt)
        {
            WaitForSeconds attacksDelay = new(.4f);

            //damage
            int hits = Random.Range(evt.attackEvent.minHits, evt.attackEvent.maxHits + 1);
            for (int i = 0; i < hits; i++)
            {
                yield return evt.attackEvent.DealDamage();
                if (i < hits - 1) yield return attacksDelay;
            }

            if (evt.attackEvent.damageDealt > 0 && evt.attackEvent.maxHits > evt.attackEvent.minHits)
                yield return Announcer.Announce($"It hit {hits} times!", holdTime: 1);
        }
    }
}