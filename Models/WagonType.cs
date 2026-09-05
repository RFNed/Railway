using System;

namespace RailwayApp.Models;

public class WagonType
{
    public int WagonTypeId { get; set; }
    public string WagonCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class NewWagonType
{
    public string WagonCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UpdateWagonType
{
    public string WagonCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
