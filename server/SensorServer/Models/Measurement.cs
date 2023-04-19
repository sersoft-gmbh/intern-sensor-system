namespace SensorServer.Models;

public class Measurement
{
    public long? Id { get; set; }
    public DateTime Date { get; set; }
    public string Location { get; set; } = null!;
    public double TemperatureCelsius { get; set; }
    public double HumidityPercent { get; set; }
}