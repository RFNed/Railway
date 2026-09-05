using RailwayApp.Services;
using Xunit;
namespace Railway.Tests;

public class TrainAlgorithmsTests
{
    [Fact]
    public void CalculateWagonLoadStatistics_ReturnsCountsAndPercentages()
    {
        var result = TrainAlgorithms.CalculateWagonLoadStatistics([true, false, true, false, false]);

        Assert.Equal(2, result.LoadedCount);
        Assert.Equal(3, result.EmptyCount);
        Assert.Equal(40m, result.LoadedPercent);
        Assert.Equal(60m, result.EmptyPercent);
    }

    [Fact]
    public void CalculateWagonLoadStatistics_EmptyCollection_ReturnsZeros()
    {
        var result = TrainAlgorithms.CalculateWagonLoadStatistics(Array.Empty<bool>());

        Assert.Equal(0, result.LoadedCount);
        Assert.Equal(0, result.EmptyCount);
        Assert.Equal(0m, result.LoadedPercent);
        Assert.Equal(0m, result.EmptyPercent);
    }

    [Fact]
    public void CalculateManagerStatistics_ReturnsManagerShare()
    {
        var result = TrainAlgorithms.CalculateManagerStatistics(3, 8);

        Assert.Equal(3, result.ManagerTrainCount);
        Assert.Equal(8, result.TotalTrainCount);
        Assert.Equal(37.5m, result.ManagerPercent);
    }

    [Fact]
    public void CalculateManagerStatistics_ZeroTotal_ReturnsZeroPercent()
    {
        var result = TrainAlgorithms.CalculateManagerStatistics(0, 0);

        Assert.Equal(0m, result.ManagerPercent);
    }

    [Fact]
    public void CalculateManagerStatistics_RejectsImpossibleValues()
    {
        Assert.Throws<ArgumentException>(() => TrainAlgorithms.CalculateManagerStatistics(5, 4));
        Assert.Throws<ArgumentOutOfRangeException>(() => TrainAlgorithms.CalculateManagerStatistics(-1, 4));
    }
}
