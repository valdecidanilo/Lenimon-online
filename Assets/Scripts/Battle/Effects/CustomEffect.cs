using System;
using Battle;
using System.Collections;

public class CustomEffect : Effect
{
    private CoroutineAction<BattleEvent> effect = new();
    public CustomEffect(Func<BattleEvent,IEnumerator> customEffect) { effect.RegisterCallback(customEffect); }
    public override IEnumerator EffectSequence(BattleEvent evt) { yield return effect?.Invoke(evt); }
}