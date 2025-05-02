using Newtonsoft.Json;
using System.Collections.Generic;

public class MoveData : ApiData
{
    public int? power;
    public int? accuracy;
    public int pp;
    public int priority;
    [JsonProperty("effect_chance")] public int? effectChance;
    [JsonProperty("damage_class")] public ApiReference typeOfMove;
    public ApiReference type;
    [JsonProperty("flavor_text_entries")] public List<FlavorText> flavorTexts;

    //Not included in the api
    public MoveTypeData moveTypeData;
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

public class MoveReference
{
    public ApiReference move { get; set; }
    [JsonProperty("version_group_details")] public List<LearningDetail> learningDetails { get; set; }
}
public class LearningDetail
{
    [JsonProperty("level_learned_at")] public int level { get; set; }
    [JsonProperty("move_learn_method")] public ApiReference refLearnMethod { get; set; }
    public int? order { get; set; }
    public ApiReference version_group { get; set; }

    //Not included in the api
    public MoveLearnMethod learnMethod => refLearnMethod.name switch
    {
        "level-up" => MoveLearnMethod.LevelUp,
        "egg" => MoveLearnMethod.Egg,
        "tutor" => MoveLearnMethod.Tutor,
        "machine" => MoveLearnMethod.TM,
        _ => MoveLearnMethod.Unknow
    };
}
public enum MoveLearnMethod
{
    LevelUp = 1,
    Egg = 2,
    Tutor = 3,
    TM = 4,
    Unknow = 999,
}