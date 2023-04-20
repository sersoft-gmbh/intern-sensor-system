using SensorServer.Helpers;

namespace SensorServer.Models;

public class MeasurementStatistics
{
    public double AverageTemperatureCelsius { get; init; }
    public double AverageTemperatureFahrenheit => AverageTemperatureCelsius.ToFahrenheit();
    public double AverageHumidityPercent { get; init; }
    
    public Measurement? MinTemperature { get; init; }
    public Measurement? MaxTemperature { get; init; }
    
    public Measurement? MinHumidity { get; init; }
    public Measurement? MaxHumidity { get; init; }
    
    public Measurement? MedianTemperature { get; init; }
    public Measurement? MedianHumidity { get; init; }
}
