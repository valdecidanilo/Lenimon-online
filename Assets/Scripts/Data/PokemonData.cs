// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
using Newtonsoft.Json;
using System.Collections.Generic;

public class Ability
{
    [JsonProperty("ability")] public ApiReference reference { get; set; }
    [JsonProperty("is_hidden")] public bool hidden { get; set; }
    public int slot { get; set; }
}

public class Cries
{
    public string latest { get; set; }
    public string legacy { get; set; }
}

public class GameIndex
{
    public int game_index { get; set; }
    public ApiReference version { get; set; }
}

public class GenerationI
{
    [JsonProperty("red-blue")]
    public RedBlue redblue { get; set; }
    public Yellow yellow { get; set; }
}

public class GenerationIi
{
    public Crystal crystal { get; set; }
    public Gold gold { get; set; }
    public Silver silver { get; set; }
}

public class GenerationIii
{
    public Emerald emerald { get; set; }

    [JsonProperty("firered-leafgreen")]
    public FireredLeafgreen fireredleafgreen { get; set; }

    [JsonProperty("ruby-sapphire")]
    public RubySapphire rubysapphire { get; set; }
}

public class GenerationIv
{
    [JsonProperty("diamond-pearl")]
    public DiamondPearl diamondpearl { get; set; }

    [JsonProperty("heartgold-soulsilver")]
    public HeartgoldSoulsilver heartgoldsoulsilver { get; set; }
    public Platinum platinum { get; set; }
}

public class GenerationV
{
    [JsonProperty("black-white")]
    public BlackWhite blackwhite { get; set; }
}

public class GenerationVi
{
    [JsonProperty("omegaruby-alphasapphire")]
    public OmegarubyAlphasapphire omegarubyalphasapphire { get; set; }

    [JsonProperty("x-y")]
    public XY xy { get; set; }
}

public class GenerationVii
{
    public Icons icons { get; set; }

    [JsonProperty("ultra-sun-ultra-moon")]
    public UltraSunUltraMoon ultrasunultramoon { get; set; }
}

public class GenerationViii
{
    public Icons icons { get; set; }
}

public class HeldItem
{
    public ApiReference item { get; set; }
    public List<VersionItemDetail> version_details { get; set; }
}

public class MoveData
{
    public ApiReference move { get; set; }
    public List<VersionGroupDetail> version_group_details { get; set; }
}

public class Other
{
    public DreamWorld dream_world { get; set; }
    public Home home { get; set; }

    [JsonProperty("official-artwork")]
    public OfficialArtwork officialartwork { get; set; }
    public Showdown showdown { get; set; }
}

public class PastAbility
{
    public List<Ability> abilities { get; set; }
    public ApiReference generation { get; set; }
}

public class PastType
{
    public ApiReference generation { get; set; }
    public List<TypePokemon> types { get; set; }
}

public class PokemonData
{
    public List<Ability> abilities { get; set; }
    public int base_experience { get; set; }
    public Cries cries { get; set; }
    public List<ApiReference> forms { get; set; }
    public List<GameIndex> game_indices { get; set; }
    public int height { get; set; }
    public List<HeldItem> held_items { get; set; }
    public int id { get; set; }
    public bool is_default { get; set; }
    public string location_area_encounters { get; set; }
    public List<MoveData> moves { get; set; }
    public string name { get; set; }
    public int order { get; set; }
    public List<PastAbility> past_abilities { get; set; }
    public List<PastType> past_types { get; set; }
    public ApiReference species { get; set; }
    public Sprites sprites { get; set; }
    public List<Stat> stats { get; set; }
    public List<TypePokemon> types { get; set; }
    public int weight { get; set; }

    public int hpStat => stats[0].base_stat;
    public int atkStat => stats[1].base_stat;
    public int defStat => stats[2].base_stat;
    public int sAtkStat => stats[3].base_stat;
    public int sDefStat => stats[4].base_stat;
    public int spdStat => stats[5].base_stat;
}

public class Stat
{
    public int base_stat { get; set; }
    public int effort { get; set; }
    public ApiReference stat { get; set; }
}

public class ApiReference
{
    public string name { get; set; }
    public string url { get; set; }
}

public class TypePokemon
{
    public int slot { get; set; }
    public ApiReference type { get; set; }
}

public class VersionGroupDetail
{
    public int level_learned_at { get; set; }
    public ApiReference move_learn_method { get; set; }
    public int? order { get; set; }
    public ApiReference version_group { get; set; }
}
public class VersionItemDetail
{
    public int rarity {  get; set; }
    public ApiReference version { get; set; }
}

public class Versions
{
    [JsonProperty("generation-i")]
    public GenerationI generation1 { get; set; }

    [JsonProperty("generation-ii")]
    public GenerationIi generation2 { get; set; }

    [JsonProperty("generation-iii")]
    public GenerationIii generation3 { get; set; }

    [JsonProperty("generation-iv")]
    public GenerationIv generation4 { get; set; }

    [JsonProperty("generation-v")]
    public GenerationV generation5 { get; set; }

    [JsonProperty("generation-vi")]
    public GenerationVi generation6 { get; set; }

    [JsonProperty("generation-vii")]
    public GenerationVii generation7 { get; set; }

    [JsonProperty("generation-viii")]
    public GenerationViii generation8 { get; set; }
}