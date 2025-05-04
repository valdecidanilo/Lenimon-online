using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class ItemModel : ApiData
{
    public int id => itemData.id;
    public string name;
    public int cost => itemData.cost;

    public readonly string effect;

    public Sprite sprite;

    private ItemData itemData;

    public ItemModel(ItemData data)
    {
        itemData = data;
        name = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(data.name.Replace("-", " "));
        effect = PokeAPI.SmallestFlavorText(data.flavorTexts);
    }
}
