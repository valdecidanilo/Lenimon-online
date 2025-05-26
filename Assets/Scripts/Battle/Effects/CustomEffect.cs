using System;
using Battle;
using System.Collections;

public class CustomEffect : Effect
{
    private CoroutineAction<BattleEvent> effect;
    public CustomEffect(Func<BattleEvent,IEnumerator> customEffect) { effect = new(customEffect); }
    public void AddEffect(Func<BattleEvent, IEnumerator> customEffect) => effect.RegisterCallback(customEffect);
    public override IEnumerator EffectSequence(BattleEvent evt) { yield return effect?.Invoke(evt); }
}