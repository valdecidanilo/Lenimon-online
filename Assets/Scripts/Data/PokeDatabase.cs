using AddressableAsyncInstances;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class PokeDatabase
{
    #region Database
    //api database
    public static Dictionary<string, MoveData> moves = new();
    public static Dictionary<string, MoveTypeData> moveTypes = new();
    public static Dictionary<string, AbilityData> abilities = new();
    #endregion

    #region Resources
    //resources database
    public const string maleIcon = "maleIcon";
    public const string femaleIcon = "femaleIcon";
    #endregion


    #region Utilities
    //Utility methods
    public static void SetGenderSprite(Image image, Gender gender)
    {
        image.gameObject.SetActive(gender != Gender.NonBinary);

        if (gender == Gender.NonBinary) return;
        string genderIcon = gender switch
        {
            Gender.Male => maleIcon,
            Gender.Female => femaleIcon
        };
        AAAsset<Sprite>.LoadAsset(genderIcon, (sprite) => image.sprite = sprite);
    }
    #endregion
}
