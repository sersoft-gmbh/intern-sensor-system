using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SensorServer.Helpers;
using UnitsNet;

namespace SensorServer.Models;

[Index(nameof(Date)), Index(nameof(Location))]
[Index(nameof(TemperatureCelsius)), Index(nameof(HumidityPercent)), Index(nameof(PressureHectopascals))]
public sealed class Measurement
{
    [Key]
    [Range(0, long.MaxValue)]
    public long? Id { get; set; }
    public DateTime Date { get; set; }
    [MinLength(3), MaxLength(128)]
    public string Location { get; set; } = null!;

    [JsonIgnore, NotMapped]
    public Temperature Temperature { get; set; }
    [JsonIgnore, NotMapped]
    public RelativeHumidity Humidity { get; set; }
    [JsonIgnore, NotMapped]
    public Pressure? Pressure { get; set; }

    [Range(-60, 60)]
    public double TemperatureCelsius
    {
        get => Temperature.DegreesCelsius;
        set => Temperature = Temperature.FromDegreesCelsius(value);
    }

    [Range(0, 1)]
    public double HumidityPercent
    {
        get => Humidity.Percent / 100;
        set => Humidity = RelativeHumidity.FromPercent(value * 100);
    }

    [Range(1, 1200)]
    public double? PressureHectopascals
    {
        get => Pressure?.Hectopascals;
        set => Pressure = value.HasValue ? UnitsNet.Pressure.FromHectopascals(value.Value) : null;
    }

    public double TemperatureFahrenheit => Temperature.DegreesFahrenheit;
    public double HeatIndexFahrenheit => this.CalculateHeatIndex().DegreesFahrenheit;
    public double HeatIndexCelsius => this.CalculateHeatIndex().DegreesCelsius;
}
