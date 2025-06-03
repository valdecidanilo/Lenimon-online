using UnityEngine;
using UnityEngine.UI;

public class TypeChartShowcase : MonoBehaviour
{
    [SerializeField] private Image prefab;
    [SerializeField] private Transform chart;

    private void Awake()
    {
        PokeDatabase.PreloadAssets();
        LoadingScreen.onDoneLoading += Setup;
    }

    private void Setup()
    {
        int typesCount = PokeDatabase.types.Length;
        //first row
        for (int i = 0; i < typesCount; i++)
        {
            string typeKey = PokeDatabase.types[i];
            Image img = Instantiate(prefab, chart);
            img.sprite = PokeDatabase.typeSprites[typeKey];
        }
        for (int i = 0; i < typesCount; i++)
        {
            string typeKey = PokeDatabase.types[i];
            Image img = Instantiate(prefab, chart);
            img.sprite = PokeDatabase.typeSprites[typeKey];
            var type = PokeDatabase.typeChart[typeKey];
            for (int j = 0; j < typesCount; j++)
            {
                var opposingType = PokeDatabase.typeChart[PokeDatabase.types[j]];
                float multiplier = type.GetMultiplier(opposingType);
                //Debug.Log($"{type.name} attacking {opposingType.name} : {multiplier}");
                Image multImage = Instantiate(prefab, chart);
                multImage.color = multiplier switch
                {
                    <= 0 => Color.black,
                    < 1 => Color.red,
                    > 1 => Color.green,
                    _ => Color.white
                };
            }
        }

        prefab.sprite = PokeDatabase.emptySprite;
    }
}
