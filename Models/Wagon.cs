using System;

namespace RailwayApp.Models;

public class Wagon
{
    public int WagonId { get; set; }
    public string WagonNumber { get; set; } = string.Empty;
    public int WagonTypeId { get; set; }
}
