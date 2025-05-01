using AddressableAsyncInstances;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class PokeDatabase
{
    //api database
    public static Dictionary<string, MoveTypeData> moveTypes = new();

    //resources database
    public const string maleIcon = "maleIcon";
    public const string femaleIcon = "femaleIcon";

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
}
