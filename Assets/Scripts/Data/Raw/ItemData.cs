using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class ItemData : ApiData
{
    [JsonProperty("sprites")] public ItemSprite spriteRoute;
    public List<ApiReference> attributes;
    public ApiReference category;
    public int? cost;

    [JsonProperty("effect_entries")] public List<EffectText> effectTexts;
    [JsonProperty("flavor_text_entries")] public List<FlavorText> flavorTexts;

    [JsonProperty("fling_effect")] public ApiReference flingEffect;
    [JsonProperty("fling_power")] public int? flinPower;

    //not included in the api
    public Sprite sprite;
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
public class TMData
{
    public int id;
    public ApiReference item;
    public ApiReference move;
    public ApiReference version_group;

    //not included in the api
    public ItemData itemData;
    public MoveData moveData;

    public Sprite icon => itemData?.sprite;
}