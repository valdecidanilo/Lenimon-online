using Battle;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class ItemModel : ApiData
{
    public int? cost => itemData.cost;

    public string effect { get; protected set; }
    public int amount;

    public Sprite sprite;

    public readonly ItemData itemData;

    public bool activePokemonOnly;
    public Effect battleEffect;

    public ItemModel(ItemData data, int amount = 1)
    {
        itemData = data;
        id = itemData.id;
        name = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(data.name.Replace("-", " "));
        effect = PokeAPI.SmallestFlavorText(data.flavorTexts).Replace('\n',' ');
        this.amount = amount;
    }
}

public class TMModel : ItemModel
{
    public readonly TMData data;

    public TMModel(TMData data) : base(data.itemData, 1)
    {
        sprite = data.itemData.sprite;
        this.data = data;
        for (int i = 0; i < data.itemData.effectTexts.Count; i++)
        {
            EffectText effectText = data.itemData.effectTexts[i];
            if (effectText.language.name == "en")
            {
                effect = effectText.text;
                break;
            }
        }
    }
}
