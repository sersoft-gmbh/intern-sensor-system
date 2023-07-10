namespace SensorServer.Models;

public class MeasurementCounts
{
    public long Total { get; init; }
    public Dictionary<string, long> PerLocation { get; init; } = new();
}
