using Newtonsoft.Json;
using System.Collections.Generic;

public class PokemonType : ApiData
{
    [JsonProperty("damage_relations")] public TypeRelations relations;
}

public class TypeRelations
{
    //offsensive
    [JsonProperty("double_damage_to")] public List<ApiReference> superEffective;
    [JsonProperty("half_damage_to")] public List<ApiReference> notEffective;
    [JsonProperty("no_damage_to")] public List<ApiReference> doNotAffect;
    //defensive
    [JsonProperty("double_damage_from")] public List<ApiReference> weakTo;
    [JsonProperty("half_damage_from")] public List<ApiReference> resistantTo;
    [JsonProperty("no_damage_from")] public List<ApiReference> immuneTo;
}