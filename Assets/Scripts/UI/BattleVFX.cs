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
        Image.Type originalType = overlay.type;
        float ppum = overlay.pixelsPerUnitMultiplier;
        RectTransform rect = (RectTransform)overlay.transform;
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
        overlay.type = originalType;
        overlay.pixelsPerUnitMultiplier = ppum;
        pokemon.ResetBattlePokemon();
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
        Vector2 windUpDirection = new Vector2(1f, .2f) * directionMultiplier;
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
        Vector2 backUpDirection = new Vector2(1f, .2f) * -finalScale * directionMultiplier;
        Vector2 backUpPosition = targetOriginalPosition - (targetRect.rect.size * (backUpDirection * finalScale * backUpDistance));
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

        pokemon.ResetBattlePokemon();
        target.ResetBattlePokemon();
    }

    public static IEnumerator DefaultSpecialMoveAnimation(this BattlePokemon pokemon, BattlePokemon target)
    {
        if(ReferenceEquals(target,null) || ReferenceEquals(target,pokemon)) yield break;
        
        RectTransform rect = (RectTransform)pokemon.image.transform;
        Vector3 originalRotation = rect.localEulerAngles;
        RectTransform targetRect = (RectTransform)target.image.transform;
        Vector2 targetOriginalPosition = targetRect.anchoredPosition;
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

        pokemon.ResetBattlePokemon();
        target.ResetBattlePokemon();
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

        pokemon.ResetBattlePokemon();
    }
    
    //switch const values
    //red hue
    const float maxAlphaColor = .7f;
    const float fadeDuration = .3f;
    //poke ball scale
    const float scaleUpDuration = .2f;
    const float pokeBallScale = .1f;
    //poke ball drop
    const float dropFactor = 1f;
    const float dropDuration = .2f;
    //poke ball movement
    const float spinFrequency = .5f;
    const float waveSection = .35f;
    const float movementDuration = .4f;
    const float upDistance = .45f;
    public static IEnumerator SwitchOutAnimation(this BattlePokemon pokemon, int side = 1)
    {
        RectTransform imageRect = (RectTransform)pokemon.image.transform;
        Vector2 imageScale = imageRect.lossyScale;
        Vector2 originalPosition = imageRect.anchoredPosition;
        Color color = Color.red;
        color.a = 0;
        pokemon.overlay.color = color;
        pokemon.overlay.sprite = null;
        
        //fade in color
        float time = 0;
        while (time < fadeDuration)
        {
            float scaledTime = time / fadeDuration;
            color.a = Mathf.Lerp(0, maxAlphaColor, scaledTime);
            pokemon.overlay.color = color;
            yield return null;
            time += Time.deltaTime;
        }

        color.a = maxAlphaColor;
        pokemon.overlay.color = color;
        
        //scale down to nothingness
        Vector3 scale = imageRect.localScale;
        const float scaleDownDuration = .3f;
        time = 0;
        while (time < scaleDownDuration)
        {
            float scaledTime = time / scaleDownDuration;
            imageRect.localScale = Vector3.Lerp(scale, Vector3.zero, scaledTime);
            yield return null;
            time += Time.deltaTime;
        }
        
        //scale up poke ball
        Vector3 pokeBallSize = Vector3.one * pokeBallScale;
        pokemon.overlay.color = default;
        pokemon.image.sprite = PokeDatabase.pokeBallSprite;
        time = 0;
        while (time < scaleUpDuration)
        {
            float scaledTime = time / scaleUpDuration;
            imageRect.localScale = Vector3.Lerp(Vector3.zero, pokeBallSize, scaledTime);
            yield return null;
            time += Time.deltaTime;
        }
        imageRect.localScale = pokeBallSize;
        
        //poke ball fall
        Vector2 pokeBallDirection = Vector2.up * dropFactor * imageRect.rect.size * imageRect.localScale;
        Vector2 pokeBallPosition = originalPosition - pokeBallDirection;
        time = 0;
        while (time < dropDuration)
        {
            float scaledTime = time / dropDuration;
            imageRect.anchoredPosition = Vector3.Lerp(originalPosition, pokeBallPosition, scaledTime);
            yield return null;
            time += Time.deltaTime;
        }
        
        //poke ball movement
        float movementDirection = side;
        float movementPosition = pokeBallPosition.x - (movementDirection * imageRect.rect.size.x);
        time = 0;
        while (time < movementDuration)
        {
            float scaledTime = time / movementDuration;
            float xPosition = Mathf.Lerp(pokeBallPosition.x, movementPosition, scaledTime);
            float yPosition = NumberUtil.SineWave(scaledTime, 1, waveSection) * imageRect.rect.size.y * upDistance;
            imageRect.anchoredPosition = new(xPosition, yPosition);
            imageRect.localEulerAngles = Vector3.forward * (360 * spinFrequency * scaledTime / movementDuration);
            yield return null;
            time += Time.deltaTime;
        }
        imageRect.anchoredPosition = new(movementPosition, 
            NumberUtil.SineWave(1, 1, waveSection) * imageRect.rect.size.y * upDistance);
        imageRect.localEulerAngles = Vector3.zero;
    }

    public static IEnumerator SwitchInAnimation(this BattlePokemon pokemon, Sprite newPokemonSprite, int side = 1)
    {
        pokemon.ResetBattlePokemon();
        RectTransform imageRect = (RectTransform)pokemon.image.transform;
        Vector2 imageScale = imageRect.lossyScale;
        Vector2 imageLocalScale = imageRect.localScale;
        Vector2 originalPosition = imageRect.anchoredPosition;
        Color color = Color.white;
        color.a = 0;

        //setup poke ball position
        imageRect.localScale = Vector3.one * pokeBallScale;
        //drop
        Vector2 dropPosition = originalPosition - (Vector2.up * dropFactor * imageRect.rect.size * imageRect.localScale);
        //movement
        Vector2 initialPosition = dropPosition;
        initialPosition -= new Vector2(side * imageRect.rect.size.x,
            -NumberUtil.SineWave(1, 1, waveSection) * imageRect.rect.size.y * upDistance);
        imageRect.anchoredPosition = initialPosition;

        pokemon.image.sprite = PokeDatabase.pokeBallSprite;

        //poke ball movement
        float time = movementDuration;
        while (time > 0)
        {
            float scaledTime = time / movementDuration;
            float xPosition = Mathf.Lerp(dropPosition.x, initialPosition.x, scaledTime);
            float yPosition = NumberUtil.SineWave(scaledTime, 1, waveSection) * imageRect.rect.size.y * upDistance;
            imageRect.anchoredPosition = new(xPosition, yPosition);
            imageRect.localEulerAngles = Vector3.forward * -(360 * spinFrequency * scaledTime / movementDuration);
            yield return null;
            time -= Time.deltaTime;
        }
        //lift up
        time = 0;
        while (time < dropDuration)
        {
            float scaledTime = time / dropDuration;
            imageRect.anchoredPosition = Vector2.Lerp(dropPosition, originalPosition, scaledTime);
            yield return null;
            time += Time.deltaTime;
        }

        //white flash
        const float flashDuration = fadeDuration / 2f;
        pokemon.overlay.sprite = null;
        time = 0;
        while (time < flashDuration)
        {
            float scaledTime = time / flashDuration;
            color.a = Mathf.Lerp(0, 1, scaledTime);
            pokemon.overlay.color = color;
            yield return null;
            time += Time.deltaTime;
        }

        //scale up pokemon
        imageRect.eulerAngles = Vector3.zero;
        pokemon.image.sprite = newPokemonSprite;
        time = 0;
        while (time < flashDuration)
        {
            float scaledTime = time / flashDuration;
            color.a = Mathf.Lerp(1, 0, scaledTime);
            pokemon.overlay.color = color;
            imageRect.localScale = Vector2.Lerp(Vector2.one * pokeBallScale, imageLocalScale, scaledTime);
            yield return null;
            time += Time.deltaTime;
        }
        
        pokemon.ResetBattlePokemon();
    }
}