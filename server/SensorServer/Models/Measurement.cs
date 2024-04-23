using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SensorServer.Helpers;

namespace SensorServer.Models;

[Index(nameof(Date))]
[Index(nameof(Location))]
public class Measurement
{
    public long? Id { get; set; }
    public DateTime Date { get; set; }
    [MaxLength(128)]
    public string Location { get; set; } = null!;
    public double TemperatureCelsius { get; set; }
    public double TemperatureFahrenheit => TemperatureCelsius.ToFahrenheit();
    public double HumidityPercent { get; set; }

    public double HeatIndexFahrenheit => this.CalculateHeatIndexInFahrenheit();
    public double HeatIndexCelsius => HeatIndexFahrenheit.ToCelsius();
}
