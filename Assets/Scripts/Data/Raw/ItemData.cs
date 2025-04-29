using Newtonsoft.Json;
using System.Collections.Generic;

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