namespace SensorServer.Models;

public class MeasurementCounts
{
    public int Total { get; init; }
    public Dictionary<string, int> PerLocation { get; init; } = new();
}
