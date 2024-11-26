using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SensorServer.Helpers;
using UnitsNet;

namespace SensorServer.Models;

[Description("A measurement of temperature, humidity, and pressure at a given date and location.")]
[Index(nameof(Date)), Index(nameof(Location))]
[Index(nameof(TemperatureCelsius)), Index(nameof(HumidityPercent)), Index(nameof(PressureHectopascals))]
public sealed class Measurement
{
    [Key]
    // [Range(1, long.MaxValue)]
    [Range(typeof(long), "1", "9223372036854775807")] // Needed until https://github.com/dotnet/aspnetcore/pull/59043 is merged.
    [Description("The unique identifier of the measurement.")]
    public long? Id { get; set; }

    [Required]
    [Description("The date and time of the measurement.")]
    public DateTime Date { get; set; }

    [Required]
    [MinLength(3), MaxLength(128)]
    [Description("The location of the measurement.")]
    public string Location { get; set; } = null!;

    [JsonIgnore, NotMapped]
    public Temperature Temperature { get; set; }
    [JsonIgnore, NotMapped]
    public RelativeHumidity Humidity { get; set; }
    [JsonIgnore, NotMapped]
    public Pressure? Pressure { get; set; }

    [Required]
    [Range(-60, 60)]
    [Description("The temperature in degrees Celsius.")]
    public double TemperatureCelsius
    {
        get => Temperature.DegreesCelsius;
        set => Temperature = Temperature.FromDegreesCelsius(value);
    }

    [Required]
    [Range(0, 1)]
    [Description("The humidity in percent.")]
    public double HumidityPercent
    {
        get => Humidity.Percent / 100;
        set => Humidity = RelativeHumidity.FromPercent(value * 100);
    }

    [Range(1, 1200)]
    [Description("The pressure in hectopascals.")]
    public double? PressureHectopascals
    {
        get => Pressure?.Hectopascals;
        set => Pressure = value.HasValue ? UnitsNet.Pressure.FromHectopascals(value.Value) : null;
    }

    [Description("The temperature in degrees Fahrenheit.")]
    public double TemperatureFahrenheit => Temperature.DegreesFahrenheit;

    [Description("The heat index in degrees Fahrenheit.")]
    public double HeatIndexFahrenheit => this.CalculateHeatIndex().DegreesFahrenheit;

    [Description("The heat index in degrees Celsius.")]
    public double HeatIndexCelsius => this.CalculateHeatIndex().DegreesCelsius;
}
