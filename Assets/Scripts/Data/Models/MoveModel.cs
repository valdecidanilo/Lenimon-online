using System;

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

    public MoveModel(MoveData data)
    {
        name = data.name;
        id = data.id;
        moveType = data.type.name;
        power = data.power ?? 0;
        accuracy = data.accuracy ?? 0;
        pp = data.pp;
        priority = data.priority;
        typeOfMove = data.moveTypeData.id;
    }
}
