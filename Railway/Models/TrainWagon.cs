using System;

namespace RailwayApp.Models;

public class TrainWagon
{
    public int TrainWagonId { get; set; }
    public int TrainId { get; set; }
    public int WagonId { get; set; }
    public bool IsLoaded { get; set; }
}

public class NewTrainWagon
{
    public int TrainId { get; set; }
    public int WagonId { get; set; }
    public bool IsLoaded { get; set; }
}

public class UpdateTrainWagon
{
    public int TrainId { get; set; }
    public int WagonId { get; set; }
    public bool IsLoaded { get; set; }
}
