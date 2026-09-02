using System;

namespace RailwayApp.Models;

public class TrainItem
{
    public int TrainId { get; set; }
    public string TrainNumber { get; set; } = string.Empty;
    public int DepartureCityId { get; set; }
    public string DepartureCityName { get; set; } = string.Empty;
    public int ArrivalCityId { get; set; }
    public string ArrivalCityName { get; set; } = string.Empty;
    public DateTime FormationDatetime { get; set; }
    public int ManagerId { get; set; }
    public string ManagerName { get; set; } = string.Empty;
    public int DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public int AssistantId { get; set; }
    public string AssistantName { get; set; } = string.Empty;
}

public class LookupItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}