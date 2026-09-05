using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace RailwayApp.Models;

public class CompositionNode
{
    public string Title { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public ObservableCollection<CompositionNode> Children { get; set; } = [];
}

public class TrainComposition
{
    public List<Locomotive> Locomotives { get; set; } = [];
    public List<WagonWithType> Wagons { get; set; } = [];
    public List<TrainWagon> TrainWagons { get; set; } = [];
}

public class WagonCompositionItem
{
    public int TrainWagonId { get; set; }
    public int WagonId { get; set; }
    public string WagonNumber { get; set; } = string.Empty;
    public string WagonTypeCode { get; set; } = string.Empty;
    public string TypeDescription { get; set; } = string.Empty;
    public bool IsLoaded { get; set; }
    public string LoadStatus => IsLoaded ? "Загружен" : "Пустой";
}
