using Newtonsoft.Json;
using System.Collections.Generic;

public class ApiReference
{
    public string name { get; set; }
    public string url { get; set; }
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