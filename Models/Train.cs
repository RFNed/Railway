using System;

namespace RailwayApp.Models;

public class Train
{
    public int TrainId { get; set; }
    public string TrainNumber { get; set; } = string.Empty;
    public int DepartureCityId { get; set; }
    public int ArrivalCityId { get; set; }
    public DateTime FormationDatetime { get; set; }
    public int ManagerId { get; set; }
    public int DriverId { get; set; }
    public int AssistantId { get; set; }
}

public class TrainDetails : Train
{
    public string DepartureCityName { get; set; } = string.Empty;
    public string ArrivalCityName { get; set; } = string.Empty;
    public string ManagerName { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
    public string AssistantName { get; set; } = string.Empty;
}

public class NewTrain
{
    public string TrainNumber { get; set; } = string.Empty;
    public int DepartureCityId { get; set; }
    public int ArrivalCityId { get; set; }
    public DateTime FormationDatetime { get; set; }
    public int ManagerId { get; set; }
    public int DriverId { get; set; }
    public int AssistantId { get; set; }
}

public class UpdateTrain
{
    public string TrainNumber { get; set; } = string.Empty;
    public int DepartureCityId { get; set; }
    public int ArrivalCityId { get; set; }
    public DateTime FormationDatetime { get; set; }
    public int ManagerId { get; set; }
    public int DriverId { get; set; }
    public int AssistantId { get; set; }
}

public class LookupItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}