using System.Collections;

namespace Battle
{
    public class CreatorStatusChangeEffect : EffectCreation
    {
        public override void AddEffect(MoveModel move)
        {
            move.effect = new StatChangeEffect();
        }
    }
}