using System.Collections;

namespace Battle
{
    public class StatusChangeMoveEffect : MoveEffectCreator
    {
        public override void AddEffect(MoveModel move)
        {
            move.effect = new StatChangeEffect(move.Data.statChanges);
        }
    }
}