using System.ComponentModel;

namespace SensorServer.Models;

[Description("The counts of measurements.")]
public sealed class MeasurementCounts
{
    [Description("The total number of measurements.")]
    public required long Total { get; init; }
    [Description("The number of measurements per location.")]
    public required Dictionary<string, long> PerLocation { get; init; }
}
