using System.Collections;
using System.Collections.Generic;
using LenixSO.Logger;
using UnityEngine;
using Logger = LenixSO.Logger.Logger;

namespace Battle
{
    public abstract class MoveEffectCreator
    {
        public static void AddEffectToMove(MoveModel move)
        {
            MoveEffectCreator effectCreation = null;
            switch (move.Data.meta.category.name)
            {
                case "damage":
                    //Logger.Log("damage move", LogFlags.DataCheck);
                    effectCreation = new DamageMoveEffect();
                    break;
                case "damage+ailment":
                    //Logger.Log("damage and ailment move", LogFlags.DataCheck);
                    break;
                case "damage+heal":
                    //Logger.Log("damage and heal move", LogFlags.DataCheck);
                    effectCreation = new DamageMoveEffect(new HealEffect
                        (move.Data.meta.drain, HealEffect.HealType.Drain),
                        subEffectSetup: (evt) => evt.target = evt.origin);
                    break;
                case "damage+lower":
                    //Logger.Log("damage and target's stats move", LogFlags.DataCheck);
                    effectCreation = new DamageMoveEffect(new StatChangeEffect
                        (move.Data.statChanges), move.Data.meta.statChance ?? 0);
                    break;
                case "damage+raise":
                    //Logger.Log("damage and self stats move", LogFlags.DataCheck);
                    effectCreation = new DamageMoveEffect(new StatChangeEffect
                        (move.Data.statChanges), move.Data.meta.statChance ?? 0,
                        (evt) => evt.target = evt.origin);
                    break;
                case "ailment":
                    //Logger.Log("ailment move", LogFlags.DataCheck);
                    break;
                case "net-good-stats":
                    effectCreation = new StatusChangeMoveEffect();
                    break;
                case "heal":
                    //Logger.Log("heal move", LogFlags.DataCheck);
                    effectCreation = new HealMoveEffect();
                    break;
                case "swagger":
                    //Logger.Log("swagger move", LogFlags.DataCheck);
                    break;
                case "ohko":
                    //Logger.Log("ohko move", LogFlags.DataCheck);
                    break;
                case "whole-field-effect":
                    //Logger.Log("whole field move", LogFlags.DataCheck);
                    break;
                case "field-effect":
                    //Logger.Log("field move", LogFlags.DataCheck);
                    break;
                case "force-switch":
                    //Logger.Log("force switch move", LogFlags.DataCheck);
                    break;
                default:
                    //unique
                    break;
            }
            effectCreation?.AddEffect(move);
        }

        public abstract void AddEffect(MoveModel move);
    }
}