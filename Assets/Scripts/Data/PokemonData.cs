// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
using Newtonsoft.Json;
using System.Collections.Generic;

public class Ability
{
    [JsonProperty("ability")] public AbilityReference reference { get; set; }
    [JsonProperty("is_hidden")] public bool hidden { get; set; }
    public int slot { get; set; }
}

public class AbilityReference
{
    public string name { get; set; }
    public string url { get; set; }
}

public class Cries
{
    public string latest { get; set; }
    public string legacy { get; set; }
}

public class Form
{
    public string name { get; set; }
    public string url { get; set; }
}

public class GameIndex
{
    public int game_index { get; set; }
    public Version version { get; set; }
}

public class Generation
{
    public string name { get; set; }
    public string url { get; set; }
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
    public ItemReference item { get; set; }
    public List<VersionItemDetail> version_details { get; set; }
}

public class ItemReference
{
    public string name { get; set; }
    public string url { get; set; }
}

public class Move
{
    public MoveReference move { get; set; }
    public List<VersionGroupDetail> version_group_details { get; set; }
}

public class MoveReference
{
    public string name { get; set; }
    public string url { get; set; }
}

public class MoveLearnMethod
{
    public string name { get; set; }
    public string url { get; set; }
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
    public Generation generation { get; set; }
}

public class PastType
{
    public Generation generation { get; set; }
    public List<TypePokemon> types { get; set; }
}

public class Pokemon
{
    public List<Ability> abilities { get; set; }
    public int base_experience { get; set; }
    public Cries cries { get; set; }
    public List<Form> forms { get; set; }
    public List<GameIndex> game_indices { get; set; }
    public int height { get; set; }
    public List<HeldItem> held_items { get; set; }
    public int id { get; set; }
    public bool is_default { get; set; }
    public string location_area_encounters { get; set; }
    public List<Move> moves { get; set; }
    public string name { get; set; }
    public int order { get; set; }
    public List<PastAbility> past_abilities { get; set; }
    public List<PastType> past_types { get; set; }
    public Species species { get; set; }
    public Sprites sprites { get; set; }
    public List<Stat> stats { get; set; }
    public List<TypePokemon> types { get; set; }
    public int weight { get; set; }
}

public class Species
{
    public string name { get; set; }
    public string url { get; set; }
}

public class Stat
{
    public int base_stat { get; set; }
    public int effort { get; set; }
    public StatReference stat { get; set; }
}

public class StatReference
{
    public string name { get; set; }
    public string url { get; set; }
}

public class TypePokemon
{
    public int slot { get; set; }
    public TypeReference type { get; set; }
}

public class TypeReference
{
    public string name { get; set; }
    public string url { get; set; }
}

public class Version
{
    public string name { get; set; }
    public string url { get; set; }
}

public class VersionGroup
{
    public string name { get; set; }
    public string url { get; set; }
}

public class VersionGroupDetail
{
    public int level_learned_at { get; set; }
    public MoveLearnMethod move_learn_method { get; set; }
    public int? order { get; set; }
    public VersionGroup version_group { get; set; }
}
public class VersionItemDetail
{
    public int rarity {  get; set; }
    public Version version { get; set; }
}

public class Versions
{
    [JsonProperty("generation-i")]
    public GenerationI generationi { get; set; }

    [JsonProperty("generation-ii")]
    public GenerationIi generationii { get; set; }

    [JsonProperty("generation-iii")]
    public GenerationIii generationiii { get; set; }

    [JsonProperty("generation-iv")]
    public GenerationIv generationiv { get; set; }

    [JsonProperty("generation-v")]
    public GenerationV generationv { get; set; }

    [JsonProperty("generation-vi")]
    public GenerationVi generationvi { get; set; }

    [JsonProperty("generation-vii")]
    public GenerationVii generationvii { get; set; }

    [JsonProperty("generation-viii")]
    public GenerationViii generationviii { get; set; }
}