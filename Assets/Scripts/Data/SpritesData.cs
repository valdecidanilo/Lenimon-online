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

public class RedBlue
{
    public string back_default { get; set; }
    public string back_gray { get; set; }
    public string back_transparent { get; set; }
    public string front_default { get; set; }
    public string front_gray { get; set; }
    public string front_transparent { get; set; }
}

public class Yellow
{
    public string back_default { get; set; }
    public string back_gray { get; set; }
    public string back_transparent { get; set; }
    public string front_default { get; set; }
    public string front_gray { get; set; }
    public string front_transparent { get; set; }
}

public class Silver
{
    public string back_default { get; set; }
    public string back_shiny { get; set; }
    public string front_default { get; set; }
    public string front_shiny { get; set; }
    public string front_transparent { get; set; }
}

public class Gold
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

public class RubySapphire
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

public class FireredLeafgreen
{
    public string back_default { get; set; }
    public string back_shiny { get; set; }
    public string front_default { get; set; }
    public string front_shiny { get; set; }
}

public class DiamondPearl
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

public class Platinum
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

public class HeartgoldSoulsilver
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

public class XY
{
    public string front_default { get; set; }
    public string front_female { get; set; }
    public string front_shiny { get; set; }
    public string front_shiny_female { get; set; }
}

public class OmegarubyAlphasapphire
{
    public string front_default { get; set; }
    public string front_female { get; set; }
    public string front_shiny { get; set; }
    public string front_shiny_female { get; set; }
}

public class UltraSunUltraMoon
{
    public string front_default { get; set; }
    public object front_female { get; set; }
    public string front_shiny { get; set; }
    public object front_shiny_female { get; set; }
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