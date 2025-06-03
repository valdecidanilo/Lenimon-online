using Newtonsoft.Json;
using System.Collections.Generic;

public class ApiLink
{
    public string url { get; set; }
}

public class ApiReference : ApiLink
{
    public string name { get; set; }
}
public class ApiData
{
    public int id { get; set; }
    public string name { get; set; }
}

public class GameIndex
{
    public int game_index { get; set; }
    public ApiReference version { get; set; }
}

public class FlavorText
{
    [JsonProperty("flavor_text")] public string text { get; set; }
    public ApiReference language { get; set; }
}

public class ItemFlavorText
{
    public string text { get; set; }
    public ApiReference language { get; set; }
}

public class EffectText
{
    [JsonProperty("effect")] public string text { get; set; }
    [JsonProperty("short_effect")] public string shortText { get; set; }
    public ApiReference language { get; set; }
}

public class ApiRequestList
{
    public List<ApiReference> results;
}