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

    public static IEnumerator MoveAnimation(this BattlePokemon pokemon, MoveType moveType)
    {
        switch (moveType)
        {
            case MoveType.Physical:
                yield return DefaultPhysicalMoveAnimation(pokemon);
                break;
            case MoveType.Special:
                yield return DefaultSpecialMoveAnimation(pokemon);
                break;
            case MoveType.Status:
                yield return DefaultStatusMoveAnimation(pokemon);
                break;
        }
    }

    public static IEnumerator DefaultPhysicalMoveAnimation(this BattlePokemon pokemon)
    {
        yield break;
    }

    public static IEnumerator DefaultSpecialMoveAnimation(this BattlePokemon pokemon)
    {
        yield break;
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
        while (time <= totalDuration)
        {
            float angle = NumberUtil.SineWave(time, amp, 1 / duration);
            rect.localEulerAngles = Vector3.forward * angle;
            yield return null;
            time += Time.deltaTime;
        }

        rect.localEulerAngles = originalAngles;
    }
}
