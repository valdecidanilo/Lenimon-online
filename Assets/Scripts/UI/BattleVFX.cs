using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class BattleVFX
{
    public static IEnumerator LerpHpBar(Pokemon pokemon, int initialValue, int currentValue, Image bar, TMP_Text text = null)
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
}
