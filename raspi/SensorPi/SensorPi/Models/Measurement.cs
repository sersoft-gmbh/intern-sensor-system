using System.ComponentModel.DataAnnotations;

namespace SensorPi.Models;

public sealed class Measurement(DateTime? dateTime, string location, double temperatureCelsius, double humidityPercent) {
    public DateTime Date { get; set; } = dateTime ?? DateTime.Now;
    [MaxLength(128)]
    public string Location { get; set; } = location;
    public double TemperatureCelsius { get; set; } = temperatureCelsius;
    public double HumidityPercent { get; set; } = humidityPercent;
}