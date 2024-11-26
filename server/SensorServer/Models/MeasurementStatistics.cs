using System.ComponentModel;
using UnitsNet;

namespace SensorServer.Models;

[Description("The statistics of measurements.")]
public sealed class MeasurementStatistics(
    Temperature? averageTemperature = null,
    RelativeHumidity? averageHumidity = null,
    Pressure? averagePressure = null)
{
    [Description("The average temperature in degrees Celsius.")]
    public double? AverageTemperatureCelsius { get; private init; } = averageTemperature?.DegreesCelsius;
    [Description("The average temperature in degrees Fahrenheit.")]
    public double? AverageTemperatureFahrenheit { get; private init; } = averageTemperature?.DegreesFahrenheit;
    [Description("The average humidity in percent.")]
    public double? AverageHumidityPercent { get; private init;  } = averageHumidity?.Percent / 100;
    [Description("The average pressure in hectopascals.")]
    public double? AveragePressureHectopascals { get; private init; } = averagePressure?.Hectopascals;

    [Description("The minimum temperature measurement.")]
    public Measurement? MinTemperature { get; init; }
    [Description("The median temperature measurement.")]
    public Measurement? MedianTemperature { get; init; }
    [Description("The maximum temperature measurement.")]
    public Measurement? MaxTemperature { get; init; }

    [Description("The minimum humidity measurement.")]
    public Measurement? MinHumidity { get; init; }
    [Description("The median humidity measurement.")]
    public Measurement? MedianHumidity { get; init; }
    [Description("The maximum humidity measurement.")]
    public Measurement? MaxHumidity { get; init; }

    [Description("The minimum pressure measurement.")]
    public Measurement? MinPressure { get; init; }
    [Description("The median pressure measurement.")]
    public Measurement? MedianPressure { get; init; }
    [Description("The maximum pressure measurement.")]
    public Measurement? MaxPressure { get; init; }
}
