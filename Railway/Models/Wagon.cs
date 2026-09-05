using System;

namespace RailwayApp.Models;

public class Wagon
{
    public int WagonId { get; set; }
    public string WagonNumber { get; set; } = string.Empty;
    public int WagonTypeId { get; set; }
}

public class WagonWithType : Wagon
{
    public string WagonTypeCode { get; set; } = string.Empty;
    public string TypeDescription { get; set; } = string.Empty;
}

public class NewWagon
{
    public string WagonNumber { get; set; } = string.Empty;
    public int WagonTypeId { get; set; }
}

public class UpdateWagon
{
    public string WagonNumber { get; set; } = string.Empty;
    public int WagonTypeId { get; set; }
}
