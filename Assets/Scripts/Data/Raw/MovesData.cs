using Newtonsoft.Json;
using System.Collections.Generic;

public class MoveData
{
    public int id;
    public string name;
    public string power;
    public string accuracy;
    public string pp;
    public string priority;
    [JsonProperty("effect_chance")] public int? effectChance;
    [JsonProperty("damage_class")] public ApiReference typeOfMove;
    public ApiReference type;
}

public class EffectDescription
{
    [JsonProperty("short_effect")] public string text;
    public ApiReference language;
}

public class MoveTypeData
{
    public MoveType id;
    public string name;
}
public enum MoveType
{
    Status = 1,
    Physical = 2,
    Special = 3,
}