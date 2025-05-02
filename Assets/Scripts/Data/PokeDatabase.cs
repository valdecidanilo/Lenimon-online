using AddressableAsyncInstances;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public static class PokeDatabase
{
    //api database
    #region Database
    public static Dictionary<string, MoveData> moves = new();
    public static Dictionary<string, MoveTypeData> moveTypes = new();
    public static Dictionary<string, AbilityData> abilities = new();
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
    #endregion

    //Utility methods
    #region Resources
    public static Sprite maleIcon;
    public static Sprite femaleIcon;
    public static Dictionary<string, Sprite> typeSprites = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void PreloadAssets()
    {
        //gender sprites
        AAAsset<Sprite>.LoadAsset(maleIconKey, (sprite) => maleIcon = sprite);
        AAAsset<Sprite>.LoadAsset(femaleIconKey, (sprite) => femaleIcon = sprite);

        //type sprites
        for (int i = 0; i < types.Length; i++)
        {
            string typeKey = types[i];
            AAAsset<Sprite>.LoadAsset($"type[{typeKey}]", (sprite) => typeSprites[typeKey] = sprite);
        }
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
    #endregion
}
