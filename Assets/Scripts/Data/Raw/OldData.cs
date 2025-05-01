using Newtonsoft.Json;
using System.Collections.Generic;

public class PastAbility
{
    public List<AbilityReference> abilities { get; set; }
    public ApiReference generation { get; set; }
}

public class PastType
{
    public ApiReference generation { get; set; }
    public List<TypePokemon> types { get; set; }
}