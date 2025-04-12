// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
using Newtonsoft.Json;
using System.Collections.Generic;

public class Icons
{
    public string front_default { get; set; }
    public string front_female { get; set; }
}

public class Sprites : SpriteTypes
{
    public Other other { get; set; }
    public Versions versions { get; set; }
}

public class SpriteTypes : Icons
{
    public string back_default { get; set; }
    public string back_female { get; set; }
    public string back_shiny { get; set; }
    public string back_shiny_female { get; set; }
    public string front_shiny { get; set; }
    public string front_shiny_female { get; set; }
}

public class FullSpriteTypes : SpriteTypes
{
    public string back_gray { get; set; }
    public string back_transparent { get; set; }
    public string back_shiny_transparent { get; set; }
    public string front_gray { get; set; }
    public string front_transparent { get; set; }
    public string front_shiny_transparent { get; set; }
}

public class BlackWhite : FullSpriteTypes
{
    public FullSpriteTypes animated { get; set; }
}

//generations
public class Generation1
{
    [JsonProperty("red-blue")]
    public FullSpriteTypes redblue { get; set; }
    public FullSpriteTypes yellow { get; set; }
}

public class Generation2
{
    public FullSpriteTypes gold { get; set; }
    public FullSpriteTypes silver { get; set; }
    public FullSpriteTypes crystal { get; set; }
}

public class Generation3
{
    public FullSpriteTypes emerald { get; set; }

    [JsonProperty("firered-leafgreen")]
    public FullSpriteTypes fireredleafgreen { get; set; }

    [JsonProperty("ruby-sapphire")]
    public FullSpriteTypes rubysapphire { get; set; }
}

public class Generation4
{
    [JsonProperty("diamond-pearl")]
    public FullSpriteTypes diamondpearl { get; set; }

    [JsonProperty("heartgold-soulsilver")]
    public FullSpriteTypes heartgoldsoulsilver { get; set; }
    public FullSpriteTypes platinum { get; set; }
}

public class Generation5
{
    [JsonProperty("black-white")]
    public BlackWhite blackwhite { get; set; }
}

public class Generation6
{
    [JsonProperty("omegaruby-alphasapphire")]
    public FullSpriteTypes omegarubyalphasapphire { get; set; }

    [JsonProperty("x-y")]
    public FullSpriteTypes xy { get; set; }
}

public class Generation7
{
    public Icons icons { get; set; }

    [JsonProperty("ultra-sun-ultra-moon")]
    public FullSpriteTypes ultrasunultramoon { get; set; }
}

public class Generation8
{
    public Icons icons { get; set; }
}

//others
public class Versions
{
    [JsonProperty("generation-i")]
    public Generation1 gen1 { get; set; }

    [JsonProperty("generation-ii")]
    public Generation2 gen2 { get; set; }

    [JsonProperty("generation-iii")]
    public Generation3 gen3 { get; set; }

    [JsonProperty("generation-iv")]
    public Generation4 gen4 { get; set; }

    [JsonProperty("generation-v")]
    public Generation5 gen5 { get; set; }

    [JsonProperty("generation-vi")]
    public Generation6 gen6 { get; set; }

    [JsonProperty("generation-vii")]
    public Generation7 gen7 { get; set; }

    [JsonProperty("generation-viii")]
    public Generation8 gen8 { get; set; }
}

public class Other
{
    public DreamWorld dream_world { get; set; }
    public Home home { get; set; }

    [JsonProperty("official-artwork")]
    public OfficialArtwork officialartwork { get; set; }
    public Showdown showdown { get; set; }
}

public class OfficialArtwork
{
    public string front_default { get; set; }
    public string front_shiny { get; set; }
}

public class Home
{
    public string front_default { get; set; }
    public string front_female { get; set; }
    public string front_shiny { get; set; }
    public string front_shiny_female { get; set; }
}

public class DreamWorld
{
    public string front_default { get; set; }
    public string front_female { get; set; }
}

public class Showdown
{
    public string back_default { get; set; }
    public string back_female { get; set; }
    public string back_shiny { get; set; }
    public string back_shiny_female { get; set; }
    public string front_default { get; set; }
    public string front_female { get; set; }
    public string front_shiny { get; set; }
    public string front_shiny_female { get; set; }
}