namespace SensorServer.Models;

public sealed class MeasurementCounts
{
    public long Total { get; init; }
    public Dictionary<string, long> PerLocation { get; init; } = new();
}
