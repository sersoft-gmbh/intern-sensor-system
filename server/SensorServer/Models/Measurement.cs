using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SensorServer.Helpers;

namespace SensorServer.Models;

[Index(nameof(Date))]
[Index(nameof(Location))]
public sealed class Measurement
{
    [Range(0, long.MaxValue)]
    public long? Id { get; set; }
    public DateTime Date { get; set; }
    [MinLength(3), MaxLength(128)]
    public string Location { get; set; } = null!;
    [Range(-60, 60)]
    public double TemperatureCelsius { get; set; }
    public double TemperatureFahrenheit => TemperatureCelsius.ToFahrenheit();
    [Range(0, 1)]
    public double HumidityPercent { get; set; }

    public double HeatIndexFahrenheit => this.CalculateHeatIndexInFahrenheit();
    public double HeatIndexCelsius => HeatIndexFahrenheit.ToCelsius();
}
