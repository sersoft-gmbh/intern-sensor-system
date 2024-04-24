using SensorPi.Accessories;
using SensorPi.Accessories.Temperature;
using SensorPi.Models;

namespace SensorPi.Controllers;

public sealed class SensorsController<TTemperatureSensor>(TTemperatureSensor sensor, StatusLight? statusLight = null, WideDisplay? display = null) : IDisposable
where TTemperatureSensor : ITemperatureSensor
{
    private readonly TTemperatureSensor _sensor = sensor;

    private readonly WideDisplay? _display = display;
    private readonly StatusLight? _statusLight = statusLight;

    private Timer? _readingTimer;

    public void Dispose()
    {
        _readingTimer?.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task TimerTick(LocationsController locationsController, ServerController server) {
        // Set light to red and blue
        _statusLight?.SetColor(LedColor.Red.WithBlue());
        // Read current value
        var current = await _sensor.ReadCurrent();
        // If no value (null), return (turn LED off)
        if (!current.HasValue)
        {
            _statusLight?.SetColor(LedColor.Off);
            return;
        }
         // Create new Measurement from Values
        var measurement = new Measurement(
            current.Value.Date, 
            locationsController.GetCurrentLocation(),
            current.Value.Temperature.DegreesCelsius, 
            current.Value.Humidity.Percent / 100);
        _display?.WriteMeasurement(measurement);
        // Send measurement to server.
        try {
            await server.SendMeasurement(measurement);
            // Set LED to green
            _statusLight?.SetColor(LedColor.Green);
        } catch (Exception ex) {
            Console.WriteLine($"Failed to send measurement: {ex}");
            // Set LED to red
            _statusLight?.SetColor(LedColor.Red);
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
