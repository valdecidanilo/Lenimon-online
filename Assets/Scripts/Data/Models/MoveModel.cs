using System;
using System.Globalization;

[Serializable]
public class MoveModel
{
    public int id;
    public string name;
    public string moveType;
    public int power;
    public int accuracy;
    public int pp;
    public int priority;
    public MoveType typeOfMove;
    public string description;

    public readonly int maxPP;

    private MoveData data;

    public MoveModel(MoveData data)
    {
        this.data = data;
        name = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(data.name.Replace("-", " "));
        id = data.id;
        moveType = data.type.name;
        power = data.power ?? 0;
        accuracy = data.accuracy ?? 0;
        pp = data.pp;
        maxPP = pp;
        priority = data.priority;
        typeOfMove = data.moveTypeData.id;
        description = PokeAPI.SmallestFlavorText(data.flavorTexts).Replace("\n", " ");
    }
}
