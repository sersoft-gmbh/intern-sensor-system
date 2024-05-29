using UnitsNet;

namespace SensorServer.Models;

public sealed class MeasurementStatistics(
    Temperature? averageTemperature = null,
    RelativeHumidity? averageHumidity = null,
    Pressure? averagePressure = null)
{
    public double? AverageTemperatureCelsius { get; } = averageTemperature?.DegreesCelsius;
    public double? AverageTemperatureFahrenheit { get; } = averageTemperature?.DegreesFahrenheit;
    public double? AverageHumidityPercent { get; } = averageHumidity?.Percent / 100;
    public double? AveragePressureHectopascals { get; } = averagePressure?.Hectopascals;

    public Measurement? MinTemperature { get; init; }
    public Measurement? MedianTemperature { get; init; }
    public Measurement? MaxTemperature { get; init; }

    public Measurement? MinHumidity { get; init; }
    public Measurement? MedianHumidity { get; init; }
    public Measurement? MaxHumidity { get; init; }
    
    public Measurement? MinPressure { get; init; }
    public Measurement? MedianPressure { get; init; }
    public Measurement? MaxPressure { get; init; }
}
