using System;

namespace LenixSO.Logger
{
    [Flags]
    public enum LogFlags
    {
        Request = 1,
        Response = 2,
        API = 4,
        Game = 8,
        DataCheck = 16,
        PokemonBuild = 32,
        Tests = 64,
    }
}