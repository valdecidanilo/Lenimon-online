using Battle;
using System.Collections;

public class CustomEffect : Effect
{
    private IEnumerator effect;
    public CustomEffect(IEnumerator customEffect) { effect = customEffect; }
    public override IEnumerator EffectSequence(BattleEvent evt) { yield return effect; }
}