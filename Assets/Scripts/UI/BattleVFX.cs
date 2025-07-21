using System;
using System.Collections;
using Lenix.NumberUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Vector2 = UnityEngine.Vector2;

namespace UI
{
    public static class BattleVFX
    {
        public static IEnumerator LerpHpBar(Pokemon pokemon, int initialValue, int currentValue, Image bar,
            TMP_Text text = null)
        {
            const float moveTime = .6f;
            float time = 0;
            while (time < moveTime)
            {
                var currentHp = Mathf.RoundToInt(Mathf.Lerp(initialValue, currentValue, time / moveTime));
                ChangeHpBar(pokemon, bar, currentHp, text);
                yield return null;
                time += Time.deltaTime;
            }

            ChangeHpBar(pokemon, bar, currentValue, text);
        }

        public static void ChangeHpBar(Pokemon pokemon, Image hpImage, int hpValue, TMP_Text text = null)
        {
            hpValue = Mathf.Clamp(hpValue, 0, pokemon.stats.hp);
            var percentage = (float)hpValue / pokemon.stats.hp;
            hpImage.fillAmount = percentage;

            var healthState = 0;
            if (percentage <= .5f) healthState++;
            if (percentage <= .25f) healthState++;
            if (text) text.text = $"{hpValue}/{pokemon.stats.hp}";

            //change color
            hpImage.sprite = PokeDatabase.hpBars[healthState];
        }

        public static void ChangeXpBar(Pokemon pokemon, Image xpImage, int xpGainValue)
        {
            var percentage = Mathf.Clamp(xpGainValue, 0, pokemon.experience) / pokemon.experienceMax;
            xpImage.fillAmount = percentage;
        }

        private static int CalculateErraticExp(int level)
        {
            return level switch
            {
                <= 50 => Mathf.FloorToInt(Mathf.Pow(level, 3) * (100 - level) / 50f),
                <= 68 => Mathf.FloorToInt(Mathf.Pow(level, 3) * (150 - level) / 100f),
                <= 98 => Mathf.FloorToInt(Mathf.Pow(level, 3) * (1911 - 10 * level) / 500f),
                _ => Mathf.FloorToInt(Mathf.Pow(level, 3) * (160 - level) / 100f)
            };
        }

        private static int CalculateFluctuatingExp(int level)
        {
            return level switch
            {
                <= 15 => Mathf.FloorToInt(Mathf.Pow(level, 3) * (24 + (level + 1) / 3) / 50f),
                <= 36 => Mathf.FloorToInt(Mathf.Pow(level, 3) * (14 + level) / 50f),
                _ => Mathf.FloorToInt(Mathf.Pow(level, 3) * (32 + level / 2) / 50f)
            };
        }
        public static int GainExperience(int opponentLevel, int baseAllyExperience, int experience, int experienceMax, bool isTrainerPokemon = false, bool hasLuckyEgg = false)
        {
            var modifier = 1.0f;
            if (isTrainerPokemon) modifier *= 1.5f;
            if (hasLuckyEgg) modifier *= 1.5f;

            var expGained = Mathf.FloorToInt((baseAllyExperience * opponentLevel * modifier) / 7);

            experience += expGained;
            if (experience >= experienceMax)
            {
                //LevelUp();
            }
            return expGained;
        }
        public static int GetExperienceForLevel(int targetLevel, GrowthRate currentGrowthRate)
        {
            return currentGrowthRate switch
            {
                GrowthRate.Fast => Mathf.FloorToInt((4f * Mathf.Pow(targetLevel, 3)) / 5f),
                GrowthRate.MediumFast => Mathf.FloorToInt(Mathf.Pow(targetLevel, 3)),
                GrowthRate.MediumSlow => Mathf.FloorToInt((6f / 5f * Mathf.Pow(targetLevel, 3)) -
                    (15f * Mathf.Pow(targetLevel, 2)) + (100f * targetLevel) - 140f),
                GrowthRate.Slow => Mathf.FloorToInt((5f * Mathf.Pow(targetLevel, 3)) / 4f),
                GrowthRate.Erratic => CalculateErraticExp(targetLevel),
                GrowthRate.Fluctuating => CalculateFluctuatingExp(targetLevel),
                _ => Mathf.FloorToInt(Mathf.Pow(targetLevel, 3))
            };
        }
        private static void LevelUp(int currentLevel, int levelMax, int levelsGained = 1, string natureIncreasedStat = "", string natureDecreasedStat = "")
        {
            currentLevel += levelsGained;

            if (currentLevel > levelMax)
                currentLevel = levelMax;
            //stats.hpStats.currentStat = CalculateHp(stats.hpStats.baseStat, stats.hpStats.iv, stats.hpStats.effort, currentLevel);
            //RecalculateStats(stats, currentLevel, natureIncreasedStat, natureDecreasedStat);
        }
        public static IEnumerator StatChangeAnimation(this BattlePokemon pokemon, bool buff = false)
        {
            var multiplier = buff ? 1 : -1;
            var overlay = pokemon.overlay;
            var originalType = overlay.type;
            var ppum = overlay.pixelsPerUnitMultiplier;
            var rect = (RectTransform)overlay.transform;
            Vector3 originalSize = rect.sizeDelta;
            //setup
            pokemon.overlay.sprite = PokeDatabase.statChangeSprite;
            rect.localScale = Vector2.right + (Vector2.up * multiplier);
            Vector2 newSize = originalSize;
            newSize.y += rect.rect.size.y;
            rect.sizeDelta = newSize;
            rect.localPosition -= Vector3.up * (newSize.y / 2f) * multiplier;

            const float baseAlpha = .4f;
            var color = buff ? Color.green : Color.red;
            color.a = baseAlpha;
            overlay.color = color;
            overlay.type = Image.Type.Tiled;
            overlay.pixelsPerUnitMultiplier = .1f;

            AudioManager.Instance.PlayStatusChangeAudio(buff);
            //movement
            Vector2 origin = rect.localPosition;
            var destination = origin + Vector2.up * (newSize.y * multiplier);
            const float duration = .4f;
            const int cycles = 2;
            const float totalTime = duration * cycles;
            float time = 0;
            while (time < totalTime)
            {
                var scaledTime = (time / duration);
                rect.localPosition = Vector2.Lerp(origin, destination, scaledTime % duration);
                var alpha = Mathf.Clamp01(NumberUtil.DistanceFromArea(time, 0, duration, duration));
                color.a = baseAlpha * alpha;
                overlay.color = color;
                yield return null;
                time += Time.deltaTime;
            }

            //reset
            overlay.type = originalType;
            overlay.pixelsPerUnitMultiplier = ppum;
            rect.sizeDelta = originalSize;
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
                case MoveType.Item:
                case MoveType.Switch:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(moveType), moveType, null);
            }
        }

        public static IEnumerator DefaultPhysicalMoveAnimation(this BattlePokemon pokemon, BattlePokemon target)
        {
            if (ReferenceEquals(target, null) || ReferenceEquals(target, pokemon)) yield break;
        
            var rect = (RectTransform)pokemon.image.transform;
            var size = rect.rect.size;
            var originalPosition = rect.anchoredPosition;
            var targetRect = (RectTransform)target.image.transform;
            var targetOriginalPosition = targetRect.anchoredPosition;
            Vector2 scale = rect.lossyScale;
            Vector2 finalScale = Vector3.Scale(rect.lossyScale, targetRect.lossyScale);

            //windup
            var directionMultiplier = Mathf.Clamp(target.transform.localPosition.x - pokemon.transform.localPosition.x, -1, 1);
            const float windUpDistance = .15f;
            const float windUpDuration = .3f;
            var windUpDirection = new Vector2(1f, .2f) * directionMultiplier;
            var windUpPosition = originalPosition - (size * (windUpDirection * windUpDistance));
            float time = 0;
            while (time < windUpDuration)
            {
                var scaledTime = time / windUpDuration;
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
            var attackDirection = windUpDirection * -1;
            var attackPosition = originalPosition - (size * (attackDirection * attackDistance));
            time = 0;
            while (time < attackDuration)
            {
                var scaledTime = time / attackDuration;
                rect.anchoredPosition = Vector2.Lerp(windUpPosition, attackPosition, scaledTime);
                yield return null;
                time += Time.deltaTime;
            }

            //return
            AudioManager.Instance.PlayHitAudio();
            const float backUpDistance = .1f;
            var backUpDirection = new Vector2(1f, .2f) * -finalScale * directionMultiplier;
            var backUpPosition = targetOriginalPosition - (targetRect.rect.size * (backUpDirection * finalScale * backUpDistance));
            const float returnDuration = .05f;
            time = 0;
            while (time < returnDuration)
            {
                var scaledTime = time / returnDuration;
                rect.anchoredPosition = Vector2.Lerp(attackPosition, originalPosition, scaledTime);
                var backUpTime = NumberUtil.SineWave(scaledTime / 2f, 1, 1);
                targetRect.anchoredPosition = Vector2.Lerp(targetOriginalPosition, backUpPosition, backUpTime);
                yield return null;
                time += Time.deltaTime;
            }

            pokemon.ResetBattlePokemon();
            target.ResetBattlePokemon();
        }

        public static IEnumerator DefaultSpecialMoveAnimation(this BattlePokemon pokemon, BattlePokemon target)
        {
            if (ReferenceEquals(target, null) || ReferenceEquals(target, pokemon)) yield break;
        
            var rect = (RectTransform)pokemon.image.transform;
            var originalRotation = rect.localEulerAngles;
            var targetRect = (RectTransform)target.image.transform;
            var targetOriginalPosition = targetRect.anchoredPosition;
            Vector2 finalScale = Vector3.Scale(rect.lossyScale, targetRect.lossyScale);

            //windup
            var directionMultiplier = Mathf.Clamp(pokemon.transform.localPosition.x - target.transform.localPosition.x, -1, 1);
            const float windUpDistance = -25f;
            const float windUpDuration = .2f;
            var windUpDirection = windUpDistance * directionMultiplier;
        
            float time = 0;
            while (time < windUpDuration)
            {
                var scaledTime = time / windUpDuration;
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
            var attackDirection = attackDistance * directionMultiplier;
            time = 0;
            while (time < attackDuration)
            {
                var scaledTime = time / attackDuration;
                rect.localEulerAngles = Vector3.forward * Mathf.Lerp(windUpDirection, attackDirection, scaledTime);
                yield return null;
                time += Time.deltaTime;
            }

            //return
            AudioManager.Instance.PlayHitAudio();
            const float backUpDistance = .1f;
            var backUpDirection = new Vector2(1f, .2f) * finalScale * directionMultiplier;
            var backUpPosition = targetOriginalPosition - (targetRect.rect.size * (backUpDirection * finalScale * backUpDistance));
            const float returnDuration = .1f;
            time = 0;
            while (time < returnDuration)
            {
                var scaledTime = time / returnDuration;
                rect.localEulerAngles = Vector3.forward * Mathf.Lerp(attackDirection, originalRotation.z, scaledTime);
                var backUpTime = NumberUtil.SineWave(scaledTime / 2f, 1, 1);
                targetRect.anchoredPosition = Vector2.Lerp(targetOriginalPosition, backUpPosition, backUpTime);
                yield return null;
                time += Time.deltaTime;
            }

            pokemon.ResetBattlePokemon();
            target.ResetBattlePokemon();
        }

        public static IEnumerator DefaultStatusMoveAnimation(this BattlePokemon pokemon)
        {
            var rect = (RectTransform) pokemon.image.transform;

            const float duration = .5f;
            const int cycles = 2;
            const float totalDuration = duration * cycles;
        
            //wave settings
            const float amp = 5;
            float time = 0;
            while (time < totalDuration)
            {
                var angle = NumberUtil.SineWave(time, amp, 1 / duration);
                rect.localEulerAngles = Vector3.forward * angle;
                yield return null;
                time += Time.deltaTime;
            }

            pokemon.ResetBattlePokemon();
        }

        public static IEnumerator FaintAnimation(this BattlePokemon battlePokemon)
        {
            var imageRect = (RectTransform)battlePokemon.image.transform;
            var originalPosition = imageRect.anchoredPosition;
            var size = imageRect.rect.size;
        
            const float fallDuration = 1.2f;
            const float fallDistance = 3;

            var finalPosition = imageRect.anchoredPosition + (Vector2.down * size * fallDistance);
            float time = 0;
            while (time < fadeDuration)
            {
                var scaledTime = time / fallDuration;
                imageRect.anchoredPosition = Vector2.Lerp(originalPosition, finalPosition, scaledTime);
                yield return null;
                time += Time.deltaTime;
            }
            imageRect.anchoredPosition = finalPosition;
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
                var scaledTime = time / fadeDuration;
                color.a = Mathf.Lerp(0, maxAlphaColor, scaledTime);
                pokemon.overlay.color = color;
                yield return null;
                time += Time.deltaTime;
            }

            color.a = maxAlphaColor;
            pokemon.overlay.color = color;
        
            //scale down to nothingness
            var scale = imageRect.localScale;
            const float scaleDownDuration = .3f;
            time = 0;
            while (time < scaleDownDuration)
            {
                var scaledTime = time / scaleDownDuration;
                imageRect.localScale = Vector3.Lerp(scale, Vector3.zero, scaledTime);
                yield return null;
                time += Time.deltaTime;
            }
        
            //scale up poke ball
            var pokeBallSize = Vector3.one * pokeBallScale;
            pokemon.overlay.color = default;
            pokemon.image.sprite = PokeDatabase.pokeBallSprite;
            time = 0;
            while (time < scaleUpDuration)
            {
                var scaledTime = time / scaleUpDuration;
                imageRect.localScale = Vector3.Lerp(Vector3.zero, pokeBallSize, scaledTime);
                yield return null;
                time += Time.deltaTime;
            }
            imageRect.localScale = pokeBallSize;
        
            //poke ball fall
            var pokeBallDirection = Vector2.up * dropFactor * imageRect.rect.size * imageRect.localScale;
            var pokeBallPosition = originalPosition - pokeBallDirection;
            time = 0;
            while (time < dropDuration)
            {
                var scaledTime = time / dropDuration;
                imageRect.anchoredPosition = Vector3.Lerp(originalPosition, pokeBallPosition, scaledTime);
                yield return null;
                time += Time.deltaTime;
            }
        
            //poke ball movement
            float movementDirection = side;
            var movementPosition = pokeBallPosition.x - (movementDirection * imageRect.rect.size.x);
            time = 0;
            while (time < movementDuration)
            {
                var scaledTime = time / movementDuration;
                var xPosition = Mathf.Lerp(pokeBallPosition.x, movementPosition, scaledTime);
                var yPosition = NumberUtil.SineWave(scaledTime, 1, waveSection) * imageRect.rect.size.y * upDistance;
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
            var imageRect = (RectTransform)pokemon.image.transform;
            Vector2 imageLocalScale = imageRect.localScale;
            var originalPosition = imageRect.anchoredPosition;
            var color = Color.white;
            color.a = 0;

            //setup poke ball position
            imageRect.localScale = Vector3.one * pokeBallScale;
            //drop
            var dropPosition = originalPosition - (Vector2.up * dropFactor * imageRect.rect.size * imageRect.localScale);
            //movement
            var initialPosition = dropPosition;
            initialPosition -= new Vector2(side * imageRect.rect.size.x,
                -NumberUtil.SineWave(1, 1, waveSection) * imageRect.rect.size.y * upDistance);
            imageRect.anchoredPosition = initialPosition;

            pokemon.image.sprite = PokeDatabase.pokeBallSprite;

            //poke ball movement
            var time = movementDuration;
            while (time > 0)
            {
                var scaledTime = time / movementDuration;
                var xPosition = Mathf.Lerp(dropPosition.x, initialPosition.x, scaledTime);
                var yPosition = NumberUtil.SineWave(scaledTime, 1, waveSection) * imageRect.rect.size.y * upDistance;
                imageRect.anchoredPosition = new(xPosition, yPosition);
                imageRect.localEulerAngles = Vector3.forward * -(360 * spinFrequency * scaledTime / movementDuration);
                yield return null;
                time -= Time.deltaTime;
            }
            //lift up
            time = 0;
            while (time < dropDuration)
            {
                var scaledTime = time / dropDuration;
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
                var scaledTime = time / flashDuration;
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
                var scaledTime = time / flashDuration;
                color.a = Mathf.Lerp(1, 0, scaledTime);
                pokemon.overlay.color = color;
                imageRect.localScale = Vector2.Lerp(Vector2.one * pokeBallScale, imageLocalScale, scaledTime);
                yield return null;
                time += Time.deltaTime;
            }
        
            pokemon.ResetBattlePokemon();
        }
    }
}