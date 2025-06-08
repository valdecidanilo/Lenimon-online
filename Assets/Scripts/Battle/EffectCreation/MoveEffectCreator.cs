using System.Collections;
using System.Collections.Generic;
using LenixSO.Logger;
using UnityEngine;
using Logger = LenixSO.Logger.Logger;

namespace Battle
{
    public static class MoveEffectCreator
    {
        public static void AddEffectToMove(MoveModel move)
        {
            Effect moveEffect = EffectNotImplemented();
            CoroutineAction<BattleEvent> moveMessage = new(MoveMessage);
            move.effectMessage = moveMessage;
            switch (move.Data?.meta?.category?.name)
            {
                case "damage":
                    //Logger.Log("damage move", LogFlags.DataCheck);
                    moveEffect = new DamageEffect();
                    break;
                case "damage+ailment":
                    //Logger.Log("damage and ailment move", LogFlags.DataCheck);
                    moveEffect = new DamageEffect();
                    break;
                case "damage+heal":
                    //Logger.Log("damage and heal move", LogFlags.DataCheck);
                    moveEffect = new DamageEffect();
                    moveEffect.subEffect = new HealEffect(move.Data.meta.drain, HealEffect.HealType.Drain);
                    moveEffect.subEffectSetup = (evt) => evt.target = evt.origin;
                    break;
                case "damage+lower":
                    //Logger.Log("damage and target's stats move", LogFlags.DataCheck);
                    moveEffect = new DamageEffect();
                    moveEffect.subEffect = new StatChangeEffect(move.Data.statChanges);
                    moveEffect.subEffectChance = move?.Data?.meta?.statChance ?? 0;
                    break;
                case "damage+raise":
                    //Logger.Log("damage and self stats move", LogFlags.DataCheck);
                    moveEffect = new DamageEffect();
                    moveEffect.subEffect = new StatChangeEffect(move.Data.statChanges);
                    moveEffect.subEffectChance = move?.Data?.meta?.statChance ?? 0;
                    moveEffect.subEffectSetup = (evt) => evt.target = evt.origin;
                    break;
                case "ailment":
                    //Logger.Log("ailment move", LogFlags.DataCheck);
                    break;
                case "net-good-stats":
                    moveEffect = new StatChangeEffect(move.Data.statChanges);
                    break;
                case "heal":
                    //Logger.Log("heal move", LogFlags.DataCheck);
                    MoveMetaData meta = move.Data.meta;
                    moveEffect = meta.healing > 0 ?
                        new HealEffect(meta.healing, HealEffect.HealType.Hp) :
                        new HealEffect(meta.drain, HealEffect.HealType.Drain);
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
            move.effect = moveEffect;
        }

        private static IEnumerator MoveMessage(BattleEvent evt)
        {
            yield return Announcer.AnnounceCoroutine($"{evt.user.referenceText} {evt.origin.name} used {evt.move.name}.", holdTime: 1f);
        }

        private static Effect EffectNotImplemented() => new CustomEffect(NotImplementedMessage);
        private static IEnumerator NotImplementedMessage(BattleEvent evt)
        {
            evt.failed = true;
            yield return Announcer.AnnounceCoroutine("(this effect was not yet implemented)", holdTime: .4f);
        }

        public static MoveModel EmptyMove(int priority = 99)
        {
            MoveData data = new();
            data.id = -1;
            data.name = "?";
            data.pp = 1;
            data.priority = priority;
            data.type = new() { name = "unknown" };
            data.target = new() { name = "user" };
            data.moveTypeData = new() { id = MoveType.Status };
            data.flavorTexts = new() {
                new FlavorText() {
                    text = "??",
                    language = new(){name = "en"}
                }
            };
            data.meta = new() {
                category = new() { name = "unique" }
            };
        
            MoveModel model = new(data);
            model.effect = EmptyEffect();
            model.effectMessage = new(Empty);

            return model;
        }
        
        private static IEnumerator Empty(BattleEvent evt) { yield break; }
        
        public static Effect EmptyEffect() => new CustomEffect(Empty);
    }
}