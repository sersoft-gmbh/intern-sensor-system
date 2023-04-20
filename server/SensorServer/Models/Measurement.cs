using SensorServer.Helpers;

namespace SensorServer.Models;

public class Measurement
{
    public long? Id { get; set; }
    public DateTime Date { get; set; }
    public string Location { get; set; } = null!;
    public double TemperatureCelsius { get; set; }
    public double TemperatureFahrenheit => TemperatureCelsius * 1.8 + 32;
    public double HumidityPercent { get; set; }

    public double HeatIndexFahrenheit => this.CalculateHeatIndexInFahrenheit();
    public double HeatIndexCelsius => (HeatIndexFahrenheit - 32) * 0.55555;
}