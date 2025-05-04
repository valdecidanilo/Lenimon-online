using Newtonsoft.Json;
using System.Collections.Generic;

public class ItemData : ApiData
{
    [JsonProperty("sprites")] public ItemSprite sprite;
    public List<ApiReference> attributes;
    public ApiReference category;
    public int cost;

    [JsonProperty("effect_entries")] public List<EffectText> effectTexts;
    [JsonProperty("flavor_text_entries")] public List<FlavorText> flavorTexts;

    [JsonProperty("fling_effect")] public ApiReference flingEffect;
    [JsonProperty("fling_power")] public int flinPower;
}
public class HeldItem
{
    public ApiReference item { get; set; }
    public List<VersionItemDetail> version_details { get; set; }
}
public class VersionItemDetail
{
    public int rarity { get; set; }
    public ApiReference version { get; set; }
}