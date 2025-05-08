using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    public class HealMoveEffect : MoveEffectCreator
    {
        public override void AddEffect(MoveModel move)
        {
            MoveMetaData meta = move.Data.meta;
            if(meta.healing > 0)
                move.effect = new HealEffect(meta.healing, HealEffect.HealType.Hp);
            else
                move.effect = new HealEffect(meta.drain, HealEffect.HealType.Drain);
        }
    }
}