using Lenix.NumberUtilities;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class BattleVFX
{
    public static IEnumerator LerpHpBar(Pokemon pokemon, int initialValue, int currentValue, Image bar,
        TMP_Text text = null)
    {
        float moveTime = .6f;
        float time = 0;
        while (time < moveTime)
        {
            int currentHp = Mathf.RoundToInt(Mathf.Lerp(initialValue, currentValue, time / moveTime));
            ChangeHpBar(pokemon, bar, currentHp, text);
            yield return null;
            time += Time.deltaTime;
        }

        ChangeHpBar(pokemon, bar, currentValue, text);
    }

    public static void ChangeHpBar(Pokemon pokemon, Image hpImage, int hpValue, TMP_Text text = null)
    {
        hpValue = Mathf.Clamp(hpValue, 0, pokemon.stats.hp);
        float percentage = (float)hpValue / pokemon.stats.hp;
        hpImage.fillAmount = percentage;

        int healthState = 0;
        if (percentage <= .5f) healthState++;
        if (percentage <= .25f) healthState++;
        if (text) text.text = $"{hpValue}/{pokemon.stats.hp}";

        //change color
        hpImage.sprite = PokeDatabase.hpBars[healthState];
    }

    public static IEnumerator StatChangeAnimation(this BattlePokemon pokemon, bool buff = false)
    {
        int multiplier = buff ? 1 : -1;
        Image overlay = pokemon.overlay;
        Sprite originalSprite = overlay.sprite;
        Image.Type originalType = overlay.type;
        float ppum = overlay.pixelsPerUnitMultiplier;
        RectTransform rect = (RectTransform)overlay.transform;
        Vector3 originalPosition = rect.localPosition;
        Vector3 originalSize = rect.sizeDelta;
        //setup
        pokemon.overlay.sprite = PokeDatabase.statChangeSprite;
        rect.localScale = Vector2.right + (Vector2.up * multiplier);
        Vector2 newSize = originalSize;
        newSize.y += rect.rect.size.y;
        rect.sizeDelta = newSize;
        rect.localPosition -= Vector3.up * (newSize.y / 2f) * multiplier;

        const float baseAlpha = .4f;
        Color color = buff ? Color.green : Color.red;
        color.a = baseAlpha;
        overlay.color = color;
        overlay.type = Image.Type.Tiled;
        overlay.pixelsPerUnitMultiplier = .1f;

        //movement
        Vector2 origin = rect.localPosition;
        Vector2 destination = origin + Vector2.up * (newSize.y * multiplier);
        const float duration = .4f;
        const int cycles = 2;
        const float totalTime = duration * cycles;
        float time = 0;
        while (time < totalTime)
        {
            float scaledTime = (time / duration);
            rect.localPosition = Vector2.Lerp(origin, destination, scaledTime % duration);
            float alpha = Mathf.Clamp01(NumberUtil.DistanceFromArea(time, 0, duration, duration));
            color.a = baseAlpha * alpha;
            overlay.color = color;
            yield return null;
            time += Time.deltaTime;
        }

        //reset
        overlay.color = default;
        overlay.type = originalType;
        overlay.pixelsPerUnitMultiplier = ppum;
        overlay.sprite = originalSprite;
        rect.localScale = Vector3.one;
        rect.sizeDelta = originalSize;
        rect.localPosition = originalPosition;
    }

    public static IEnumerator MoveAnimation(this BattlePokemon pokemon, MoveType moveType, BattlePokemon target)
    {
        switch (moveType)
        {
            case MoveType.Physical:
                yield return DefaultPhysicalMoveAnimation(pokemon, target);
                break;
            case MoveType.Special:
                yield return DefaultSpecialMoveAnimation(pokemon, target);
                break;
            case MoveType.Status:
                yield return DefaultStatusMoveAnimation(pokemon);
                break;
        }
    }

    public static IEnumerator DefaultPhysicalMoveAnimation(this BattlePokemon pokemon, BattlePokemon target)
    {
        if(ReferenceEquals(target,null) || ReferenceEquals(target,pokemon)) yield break;
        
        RectTransform rect = (RectTransform)pokemon.image.transform;
        Vector2 size = rect.rect.size;
        Vector2 originalPosition = rect.anchoredPosition;
        RectTransform targetRect = (RectTransform)target.image.transform;
        Vector2 targetOriginalPosition = targetRect.anchoredPosition;
        Vector2 scale = rect.lossyScale;
        Vector2 finalScale = Vector3.Scale(rect.lossyScale, targetRect.lossyScale);

        //windup
        float directionMultiplier = Mathf.Clamp(target.transform.localPosition.x - pokemon.transform.localPosition.x, -1, 1);
        const float windUpDistance = .15f;
        const float windUpDuration = .3f;
        Vector2 windUpDirection = new Vector2(1f, .2f) * scale * directionMultiplier;
        Vector2 windUpPosition = originalPosition - (size * (windUpDirection * windUpDistance));
        float time = 0;
        while (time < windUpDuration)
        {
            float scaledTime = time / windUpDuration;
            rect.anchoredPosition = Vector2.Lerp(originalPosition, windUpPosition, scaledTime);
            yield return null;
            time += Time.deltaTime;
        }

        //hold
        const float holdTime = .2f;
        yield return new WaitForSeconds(holdTime);

        //attack
        const float attackDistance = .2f;
        const float attackDuration = .1f;
        Vector2 attackDirection = windUpDirection * -1;
        Vector2 attackPosition = originalPosition - (size * (attackDirection * attackDistance));
        time = 0;
        while (time < attackDuration)
        {
            float scaledTime = time / attackDuration;
            rect.anchoredPosition = Vector2.Lerp(windUpPosition, attackPosition, scaledTime);
            yield return null;
            time += Time.deltaTime;
        }

        //return
        const float backUpDistance = .1f;
        Vector2 backUpPosition = targetOriginalPosition - (targetRect.rect.size * (attackDirection * finalScale * backUpDistance));
        const float returnDuration = .05f;
        time = 0;
        while (time < returnDuration)
        {
            float scaledTime = time / returnDuration;
            rect.anchoredPosition = Vector2.Lerp(attackPosition, originalPosition, scaledTime);
            float backUpTime = NumberUtil.SineWave(scaledTime / 2f, 1, 1);
            targetRect.anchoredPosition = Vector2.Lerp(targetOriginalPosition, backUpPosition, backUpTime);
            yield return null;
            time += Time.deltaTime;
        }

        rect.anchoredPosition = originalPosition;
        targetRect.anchoredPosition = targetOriginalPosition;
    }

    public static IEnumerator DefaultSpecialMoveAnimation(this BattlePokemon pokemon, BattlePokemon target)
    {
        if(ReferenceEquals(target,null) || ReferenceEquals(target,pokemon)) yield break;
        
        RectTransform rect = (RectTransform)pokemon.image.transform;
        Vector3 originalRotation = rect.localEulerAngles;
        RectTransform targetRect = (RectTransform)target.image.transform;
        Vector2 targetOriginalPosition = targetRect.anchoredPosition;
        Vector2 scale = rect.lossyScale;
        Vector2 finalScale = Vector3.Scale(rect.lossyScale, targetRect.lossyScale);

        //windup
        float directionMultiplier = Mathf.Clamp(pokemon.transform.localPosition.x - target.transform.localPosition.x, -1, 1);
        const float windUpDistance = -25f;
        const float windUpDuration = .2f;
        float windUpDirection = windUpDistance * directionMultiplier;
        
        float time = 0;
        while (time < windUpDuration)
        {
            float scaledTime = time / windUpDuration;
            rect.localEulerAngles = Vector3.forward * Mathf.Lerp(originalRotation.z, windUpDirection, scaledTime);
            yield return null;
            time += Time.deltaTime;
        }

        //hold
        const float holdTime = .2f;
        yield return new WaitForSeconds(holdTime);

        //attack
        const float attackDistance = 10f;
        const float attackDuration = .1f;
        float attackDirection = attackDistance * directionMultiplier;
        time = 0;
        while (time < attackDuration)
        {
            float scaledTime = time / attackDuration;
            rect.localEulerAngles = Vector3.forward * Mathf.Lerp(windUpDirection, attackDirection, scaledTime);
            yield return null;
            time += Time.deltaTime;
        }

        //return
        const float backUpDistance = .1f;
        Vector2 backUpDirection = new Vector2(1f, .2f) * finalScale * directionMultiplier;
        Vector2 backUpPosition = targetOriginalPosition - (targetRect.rect.size * (backUpDirection * finalScale * backUpDistance));
        const float returnDuration = .1f;
        time = 0;
        while (time < returnDuration)
        {
            float scaledTime = time / returnDuration;
            rect.localEulerAngles = Vector3.forward * Mathf.Lerp(attackDirection, originalRotation.z, scaledTime);
            float backUpTime = NumberUtil.SineWave(scaledTime / 2f, 1, 1);
            targetRect.anchoredPosition = Vector2.Lerp(targetOriginalPosition, backUpPosition, backUpTime);
            yield return null;
            time += Time.deltaTime;
        }

        rect.localEulerAngles = originalRotation;
        targetRect.anchoredPosition = targetOriginalPosition;
    }

    public static IEnumerator DefaultStatusMoveAnimation(this BattlePokemon pokemon)
    {
        RectTransform rect = (RectTransform)pokemon.image.transform;
        Vector3 originalAngles = rect.localEulerAngles;

        const float duration = .5f;
        const int cycles = 2;
        const float totalDuration = duration * cycles;
        
        //wave settings
        const float amp = 5;

        float time = 0;
        while (time < totalDuration)
        {
            float angle = NumberUtil.SineWave(time, amp, 1 / duration);
            rect.localEulerAngles = Vector3.forward * angle;
            yield return null;
            time += Time.deltaTime;
        }

        rect.localEulerAngles = originalAngles;
    }
}