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

public class HeldItem
{
    public ApiReference item { get; set; }
    public List<VersionItemDetail> version_details { get; set; }
}

public class MoveReference
{
    public ApiReference move { get; set; }
    public List<VersionGroupDetail> version_group_details { get; set; }
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
    public List<MoveReference> moves { get; set; }
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