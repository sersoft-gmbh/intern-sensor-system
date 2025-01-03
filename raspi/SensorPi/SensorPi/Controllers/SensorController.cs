using System.ComponentModel.DataAnnotations;
using SensorPi.Accessories;
using SensorPi.Accessories.Temperature;
using SensorPi.Models;

namespace SensorPi.Controllers;

public sealed class SensorsController<TTemperatureSensor>(TTemperatureSensor sensor, StatusLight? statusLight = null, WideDisplay? display = null) : IDisposable
where TTemperatureSensor : ITemperatureSensor
{
    private readonly TTemperatureSensor _sensor = sensor;

    private Timer? _readingTimer;

    ~SensorsController() => Dispose(false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing) _readingTimer?.Dispose();
    }

    private async Task SetColor(LedColor color) 
    {
        if (statusLight == null) return;
        await statusLight.SetColor(color);
    }

    private async Task TimerTick(LocationsController locationsController, ServerController server) 
    {
        // Set light to red and blue
        await SetColor(LedColor.Red.WithBlue());
        // Read current value
        var current = await _sensor.ReadCurrent();
        // If no value (null), return (turn LED off)
        
        if (!current.HasValue)
        {
            await SetColor(LedColor.Off);
            return;
        }
         // Create new Measurement from Values
        var measurement = new Measurement(
            current.Value.Date, 
            locationsController.GetCurrentLocation(),
            current.Value.Temperature.DegreesCelsius, 
            current.Value.Humidity.Percent / 100,
            current.Value.Pressure?.Hectopascals);
        if (display != null)
            await display.WriteMeasurement(measurement);

        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(measurement, new ValidationContext(measurement, null, null), results, true))
        {
            await SetColor(LedColor.Red);
            return;
        }
        // Send measurement to server.
        try {
            await server.SendMeasurement(measurement);
            // Set LED to green
            await SetColor(LedColor.Green);
        } catch (Exception ex) {
            Console.WriteLine($"Failed to send measurement: {ex}");
            // Set LED to red
            await SetColor(LedColor.Red);
        }
    }

    public void StartReading(ServerController server, LocationsController locationsController) 
    {
        if (_readingTimer != null) StopReading();
        _readingTimer = new Timer(async _ => await TimerTick(locationsController, server), null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    public void StopReading() {
        _readingTimer?.Dispose();
        _readingTimer = null;
    }
}
