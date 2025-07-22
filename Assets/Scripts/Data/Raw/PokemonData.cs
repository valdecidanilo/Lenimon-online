using Newtonsoft.Json;
using System.Collections.Generic;
using Utils;

public class AbilityReference
{
    [JsonProperty("ability")] public ApiReference reference { get; set; }
    [JsonProperty("is_hidden")] public bool hidden { get; set; }
    public int slot { get; set; }
}
public class AbilityData : ApiData
{
    [JsonProperty("flavor_text_entries")] public List<FlavorText> flavorTexts { get; set; }

    public string abilityName;
    public string flavorText;
}

public class Cries
{
    public string latest { get; set; }
    public string legacy { get; set; }
}

public class PokemonData : ApiData
{
    public List<AbilityReference> abilities { get; set; }
    public int base_experience { get; set; }
    public Cries cries { get; set; }
    public List<ApiReference> forms { get; set; }
    public List<GameIndex> game_indices { get; set; }
    public int height { get; set; }
    public List<HeldItem> held_items { get; set; }
    public bool is_default { get; set; }
    public string location_area_encounters { get; set; }
    public List<MoveReference> moves { get; set; }
    public int order { get; set; }
    public List<PastAbility> past_abilities { get; set; }
    public List<PastType> past_types { get; set; }
    public ApiReference species { get; set; }
    public Sprites sprites { get; set; }
    public List<Stat> stats { get; set; }
    public List<TypePokemonReference> types { get; set; }
    public int weight { get; set; }

    public int hpStat => stats[(int)StatType.hp].base_stat;
    public int atkStat => stats[(int)StatType.atk].base_stat;
    public int defStat => stats[(int)StatType.def].base_stat;
    public int sAtkStat => stats[(int)StatType.sAtk].base_stat;
    public int sDefStat => stats[(int)StatType.sDef].base_stat;
    public int spdStat => stats[(int)StatType.spd].base_stat;
}

public enum StatType
{
    hp = 0,
    atk = 1,
    def = 2,
    sAtk = 3,
    sDef = 4,
    spd = 5,
    acc = 6,
    eva = 7
}

public class Stat
{
    public int base_stat { get; set; }
    public int effort { get; set; }
    public ApiReference stat { get; set; }
}

public class TypePokemonReference
{
    public int slot { get; set; }
    public ApiReference type { get; set; }
}

public class Species
{
    [JsonProperty("base_happiness")] public int baseHapiness;
    [JsonProperty("capture_rate")] public int captureRate;
    [JsonProperty("gender_rate")] public int genderRate;
    [JsonProperty("hatch_counter")] public int hatchCounter;
    [JsonProperty("is_baby")] public bool isBaby;
    [JsonProperty("is_legendary")] public bool isLegendary;
    [JsonProperty("is_mythical")] public bool isMythical;

    //references
    [JsonProperty("evolution_chain")] public ApiReference evolutionChain;
    [JsonProperty("egg_groups")] public List<ApiReference> eggGroup;
    [JsonProperty("")] public ApiReference evolvesFrom;
    [JsonProperty("growth_rate")] public ApiReference growthRate;
    public ApiReference color;
    public ApiReference habitat;
}

public class GrowthRate : ApiData
{
    [JsonProperty("levels")] public List<Levels> levels;
    public GrowthRateEnum growthRate => (GrowthRateEnum)(id + 1);
}

public class Levels
{
    [JsonProperty("experience")] public int experience { get; set; }
    [JsonProperty("level")] public int level { get; set; }
}
public class EvolutionChain
{

}

public class Evolution
{

}