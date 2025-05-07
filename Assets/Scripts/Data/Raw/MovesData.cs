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
    
    public MoveMetaData meta;
    public List<TMData> machines;
    public ApiReference target;
    [JsonProperty("stat_changes")] public List<StatChange> statChanges;

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
        _ => MoveLearnMethod.Unknown
    };
}
public enum MoveLearnMethod
{
    LevelUp = 1,
    Egg = 2,
    Tutor = 3,
    TM = 4,
    Unknown = 999,
}

public class MoveMetaData
{
    public ApiReference category;
    public int? drain; //percentage; also recoil if negative
    public int? healing; //percentage
    [JsonProperty("min_hits")] public int? min_hits; 
    [JsonProperty("max_hits")] public int? max_hits; 
    [JsonProperty("min_turns")] public int? minTurns; 
    [JsonProperty("max_turns")] public int? maxTurns; 
    [JsonProperty("crit_rate")] public int? critRate;
    [JsonProperty("flinch_chance")] public int? flinchChance; //percentage
    [JsonProperty("stat_chance")] public int? statChance; //percentage; The likelihood this attack will cause a stat change in the target Pokémon.
    [JsonProperty("ailment_chance")] public int? ailmentChance; //percentage
    public ApiReference ailment;
}
public class TMData
{
    public ApiLink machine;
    public ApiReference version_group;
}

public class StatChange
{
    public int change;
    public ApiReference stat;
}