using Battle;
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
    public delegate bool CheckUseItem(Pokemon pokemon, out string failMessage);
    public CheckUseItem checkItemUse;

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
        effect = $"Teaches {CultureInfo.InvariantCulture.TextInfo.ToTitleCase(data.move.name.Replace("-", " "))} to a compatible Pokémon.";
    }
}
