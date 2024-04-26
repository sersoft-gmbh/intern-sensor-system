using System.ComponentModel.DataAnnotations;

namespace SensorPi.Models;

public sealed class Measurement(DateTime? dateTime, string location, double temperatureCelsius, double humidityPercent, double? pressureHectopascals) {
    public DateTime Date { get; set; } = dateTime ?? DateTime.Now;
    [MinLength(3), MaxLength(128)]
    public string Location { get; set; } = location;
    [Range(-60, 60)]
    public double TemperatureCelsius { get; set; } = temperatureCelsius;
    [Range(0, 1)]
    public double HumidityPercent { get; set; } = humidityPercent;
    [Range(1, 1200)]
    public double? PressureHectopascals { get; set; } = pressureHectopascals;
}