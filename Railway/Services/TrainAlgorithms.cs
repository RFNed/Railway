using System;
using System.Collections.Generic;
namespace RailwayApp.Services;

public readonly record struct WagonLoadStatistics(
    int LoadedCount,
    int EmptyCount,
    decimal LoadedPercent,
    decimal EmptyPercent);

public readonly record struct ManagerTrainStatistics(
    int ManagerTrainCount,
    int TotalTrainCount,
    decimal ManagerPercent);

public static class TrainAlgorithms
{
    public static WagonLoadStatistics CalculateWagonLoadStatistics(IEnumerable<bool> loadedStates)
    {
        ArgumentNullException.ThrowIfNull(loadedStates);

        int loaded = 0;
        int total = 0;
        foreach (bool isLoaded in loadedStates)
        {
            total++;
            if (isLoaded) loaded++;
        }

        int empty = total - loaded;
        decimal loadedPercent = total == 0 ? 0 : Math.Round(loaded * 100m / total, 2);
        decimal emptyPercent = total == 0 ? 0 : Math.Round(empty * 100m / total, 2);
        return new WagonLoadStatistics(loaded, empty, loadedPercent, emptyPercent);
    }

    public static ManagerTrainStatistics CalculateManagerStatistics(int managerTrainCount, int totalTrainCount)
    {
        if (managerTrainCount < 0) throw new ArgumentOutOfRangeException(nameof(managerTrainCount));
        if (totalTrainCount < 0) throw new ArgumentOutOfRangeException(nameof(totalTrainCount));
        if (managerTrainCount > totalTrainCount) throw new ArgumentException("Количество составов менеджера не может превышать общее количество составов.");

        decimal percent = totalTrainCount == 0 ? 0 : Math.Round(managerTrainCount * 100m / totalTrainCount, 2);
        return new ManagerTrainStatistics(managerTrainCount, totalTrainCount, percent);
    }
}
