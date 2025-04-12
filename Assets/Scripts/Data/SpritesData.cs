// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
using Newtonsoft.Json;
using System.Collections.Generic;

public class Icons
{
    public string front_default { get; set; }
    public string front_female { get; set; }
}

public class Sprites
{
    public string back_default { get; set; }
    public string back_female { get; set; }
    public string back_shiny { get; set; }
    public string back_shiny_female { get; set; }
    public string front_default { get; set; }
    public string front_female { get; set; }
    public string front_shiny { get; set; }
    public string front_shiny_female { get; set; }
    public Other other { get; set; }
    public Versions versions { get; set; }
}

//1
public class Gen1Sprites
{
    public string back_default { get; set; }
    public string back_gray { get; set; }
    public string back_transparent { get; set; }
    public string front_default { get; set; }
    public string front_gray { get; set; }
    public string front_transparent { get; set; }
}

//2
public class Gen2Sprites
{
    public string back_default { get; set; }
    public string back_shiny { get; set; }
    public string front_default { get; set; }
    public string front_shiny { get; set; }
    public string front_transparent { get; set; }
}

public class Crystal
{
    public string back_default { get; set; }
    public string back_shiny { get; set; }
    public string back_shiny_transparent { get; set; }
    public string back_transparent { get; set; }
    public string front_default { get; set; }
    public string front_shiny { get; set; }
    public string front_shiny_transparent { get; set; }
    public string front_transparent { get; set; }
}

//3
public class Gen3Sprites
{
    public string back_default { get; set; }
    public string back_shiny { get; set; }
    public string front_default { get; set; }
    public string front_shiny { get; set; }
}

public class Emerald
{
    public string front_default { get; set; }
    public string front_shiny { get; set; }
}

//4
public class Gen4Sprites
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

//5
public class BlackWhite
{
    public AnimatedSprite animated { get; set; }
    public string back_default { get; set; }
    public string back_female { get; set; }
    public string back_shiny { get; set; }
    public string back_shiny_female { get; set; }
    public string front_default { get; set; }
    public string front_female { get; set; }
    public string front_shiny { get; set; }
    public string front_shiny_female { get; set; }
}

//6
public class Gen6Sprites
{
    public string front_default { get; set; }
    public string front_female { get; set; }
    public string front_shiny { get; set; }
    public string front_shiny_female { get; set; }
}

//7
public class UltraSunUltraMoon
{
    public string front_default { get; set; }
    public object front_female { get; set; }
    public string front_shiny { get; set; }
    public object front_shiny_female { get; set; }
}

//generations
public class GenerationI
{
    [JsonProperty("red-blue")]
    public Gen1Sprites redblue { get; set; }
    public Gen1Sprites yellow { get; set; }
}

public class GenerationIi
{
    public Gen2Sprites gold { get; set; }
    public Gen2Sprites silver { get; set; }
    public Crystal crystal { get; set; }
}

public class GenerationIii
{
    public Emerald emerald { get; set; }

    [JsonProperty("firered-leafgreen")]
    public Gen3Sprites fireredleafgreen { get; set; }

    [JsonProperty("ruby-sapphire")]
    public Gen3Sprites rubysapphire { get; set; }
}

public class GenerationIv
{
    [JsonProperty("diamond-pearl")]
    public Gen4Sprites diamondpearl { get; set; }

    [JsonProperty("heartgold-soulsilver")]
    public Gen4Sprites heartgoldsoulsilver { get; set; }
    public Gen4Sprites platinum { get; set; }
}

public class GenerationV
{
    [JsonProperty("black-white")]
    public BlackWhite blackwhite { get; set; }
}

public class GenerationVi
{
    [JsonProperty("omegaruby-alphasapphire")]
    public Gen6Sprites omegarubyalphasapphire { get; set; }

    [JsonProperty("x-y")]
    public Gen6Sprites xy { get; set; }
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

//others
public class Versions
{
    [JsonProperty("generation-i")]
    public GenerationI gen1 { get; set; }

    [JsonProperty("generation-ii")]
    public GenerationIi gen2 { get; set; }

    [JsonProperty("generation-iii")]
    public GenerationIii gen3 { get; set; }

    [JsonProperty("generation-iv")]
    public GenerationIv gen4 { get; set; }

    [JsonProperty("generation-v")]
    public GenerationV gen5 { get; set; }

    [JsonProperty("generation-vi")]
    public GenerationVi gen6 { get; set; }

    [JsonProperty("generation-vii")]
    public GenerationVii gen7 { get; set; }

    [JsonProperty("generation-viii")]
    public GenerationViii gen8 { get; set; }
}

public class Other
{
    public DreamWorld dream_world { get; set; }
    public Home home { get; set; }

    [JsonProperty("official-artwork")]
    public OfficialArtwork officialartwork { get; set; }
    public Showdown showdown { get; set; }
}

public class AnimatedSprite
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