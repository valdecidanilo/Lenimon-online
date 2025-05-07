using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    public abstract class EffectCreation
    {
        public static void AddEffectToMove(MoveModel move)
        {
            EffectCreation effectCreation = null;
            switch (move.Data.meta.category.name)
            {
                case "damage":
                    break;
                case "damage+ailment":
                    break;
                case "damage+heal":
                    break;
                case "damage+lower":
                    break;
                case "damage+raise":
                    break;
                case "ailment":
                    break;
                case "net-good-stats":
                    effectCreation = new CreatorStatusChangeEffect();
                    break;
                case "heal":
                    break;
                case "swagger":
                    break;
                case "ohko":
                    break;
                case "whole-field-effect":
                    break;
                case "field-effec":
                    break;
                case "force-switch":
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