using System.Globalization;
using Battle;

public class MoveModel
{
    public int id;
    public string name;
    public string moveTypeName => moveType.name;
    public int? power;
    public int? accuracy;
    public int pp;
    public int priority;
    public MoveType typeOfMove;
    public string description;

    public int maxPP { get; private set; }
    public TypeChartEntry moveType;

    private MoveData data;
    public MoveData Data => data;

    public Effect effect;
    public CoroutineAction<BattleEvent> effectMessage;

    public MoveModel(MoveData data)
    {
        this.data = data;
        name = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(data.name.Replace("-", " "));
        id = data.id;
        moveType = PokeDatabase.GetType(data.name == "struggle" ? "" : data.type.name);
        power = 600;// data.power;
        accuracy = data.accuracy;
        pp = 100;//data.pp;
        maxPP = pp;
        priority = data.priority;
        typeOfMove = data.moveTypeData.id;
        description = PokeAPI.SmallestFlavorText(data.flavorTexts).Replace("\n", " ");
        MoveEffectCreator.AddEffectToMove(this);
    }
}
