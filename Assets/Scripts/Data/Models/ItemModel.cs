using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class ItemModel : ApiData
{
    public int cost => itemData.cost;

    public readonly string effect;
    public int amount;

    public Sprite sprite;

    private ItemData itemData;

    public ItemModel(ItemData data, int amount = 1)
    {
        itemData = data;
        id = itemData.id;
        name = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(data.name.Replace("-", " "));
        effect = PokeAPI.SmallestFlavorText(data.flavorTexts);
        this.amount = amount;
    }
}
