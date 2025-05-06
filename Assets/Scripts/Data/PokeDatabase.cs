using AddressableAsyncInstances;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LenixSO.Logger;
using Logger = LenixSO.Logger.Logger;

public static class PokeDatabase
{
    //api database
    #region Database
    public static Dictionary<string, MoveData> moves = new();
    public static Dictionary<string, MoveTypeData> moveTypes = new();
    public static Dictionary<string, AbilityData> abilities = new();
    public static Dictionary<string, ItemData> items = new();
    public static Dictionary<string, Stats> natures = new();
    #endregion

    //resources database
    #region Addressable Keys
    private const string maleIconKey = "maleIcon";
    private const string femaleIconKey = "femaleIcon";
    private static readonly string[] types = 
    {
        "bug",
        "dark",
        "dragon",
        "electric",
        "fairy",
        "fighting",
        "fire",
        "flying",
        "ghost",
        "grass",
        "ground",
        "ice",
        "normal",
        "poison",
        "psychic",
        "rock",
        "steel",
        "unknown",
        "water",
    };
    private static readonly List<string> natureNames = new();
    #endregion

    //Utility methods
    #region Resources
    public static Sprite maleIcon;
    public static Sprite femaleIcon;
    public static Dictionary<string, Sprite> typeSprites = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void PreloadAssets()
    {
        Checklist preloadedAssets = new(1);
        LoadingScreen.AddOrChangeQueue(preloadedAssets, "Loading assets...");
        //gender sprites
        AAAsset<Sprite>.LoadAsset(maleIconKey, (sprite) =>
        {
            maleIcon = sprite;
            preloadedAssets.FinishStep();
        });
        preloadedAssets.AddStep();
        AAAsset<Sprite>.LoadAsset(femaleIconKey, (sprite) =>
        {
            femaleIcon = sprite;
            preloadedAssets.FinishStep();
        });

        //type sprites
        for (int i = 0; i < types.Length; i++)
        {
            string typeKey = types[i];
            preloadedAssets.AddStep();
            AAAsset<Sprite>.LoadAsset($"type[{typeKey}]", (sprite) =>
            {
                typeSprites[typeKey] = sprite;
                preloadedAssets.FinishStep();
            });
        }

        preloadedAssets.AddStep();
        WebConnection.GetRequest<Natures>("https://pokeapi.co/api/v2/nature", (data) =>
        {
            for (int i = 0; i < data.results.Count; i++)
            {
                string name = data.results[i].name;
                natureNames.Add(name);
                Stats nature = new(100, 100, 100, 100, 100, 100, 100, 100);
                int decreaseId = (i / 5) + 1;
                int increaseId = (i % 5) + 1;
                nature[(StatType)increaseId] += 10;
                nature[(StatType)decreaseId] -= 10;
                Logger.Log($"{name} => +{(StatType)increaseId}; -{(StatType)decreaseId}", LogFlags.DataCheck);
                natures[name] = nature;
            }
            preloadedAssets.FinishStep();
        });
    }

    public static void SetGenderSprite(Image image, Gender gender)
    {
        image.gameObject.SetActive(gender != Gender.NonBinary);
        if (gender == Gender.NonBinary) return;
        image.sprite = gender switch
        {
            Gender.Male => maleIcon,
            Gender.Female => femaleIcon
        };
    }

    public static Stats GetRandomNature(ref string name)
    {
        name = natureNames[Random.Range(0, natureNames.Count)];
        return natures[name];
    }
    #endregion

    public struct Natures
    {
        public List<ApiReference> results;
    }
}