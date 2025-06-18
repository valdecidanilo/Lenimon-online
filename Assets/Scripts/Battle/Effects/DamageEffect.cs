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
            WaitForSeconds attacksDelay = new(.6f);

            //damage
            int hits = Random.Range(evt.attackEvent.minHits, evt.attackEvent.maxHits + 1);
            for (int i = 0; i < hits; i++)
            {
                yield return evt.attackEvent.DealDamage();
                if (i < hits - 1) yield return attacksDelay;
            }

            if (evt.attackEvent.damageDealt > 0 && evt.attackEvent.maxHits > evt.attackEvent.minHits)
                yield return Announcer.AnnounceCoroutine($"It hit {hits} times!", holdTime: 1);

            if (evt.move.Data.meta == null) yield break;

            //check flinch
            int r = Random.Range(0, 101);
            if (r <= evt.move.Data.meta.flinchChance)
                evt.flinchTarget = true;

            //recoil
            if (evt.move.Data.meta == null || evt.move.Data.meta.drain >= 0 && evt.move.Data.meta.healing >= 0) yield break;
            int recoil = 0;
            if (evt.move.Data.meta.drain < 0) 
                recoil = Mathf.FloorToInt(evt.attackEvent.damageDealt * evt.move.Data.meta.drain / -100f);
            else if (evt.move.Data.meta.healing < 0)
                recoil = Mathf.FloorToInt(evt.origin.stats[StatType.hp] * evt.move.Data.meta.healing / -100f);

            if (recoil <= 0) yield break;
            yield return evt.origin.DamagePokemon(recoil);
            yield return Announcer.AnnounceCoroutine($"{evt.origin.name} was damaged by recoil!", holdTime: 1);
        }
    }
}