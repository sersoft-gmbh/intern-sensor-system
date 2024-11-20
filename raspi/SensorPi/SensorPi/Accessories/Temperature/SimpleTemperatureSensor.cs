
using System.Device.Gpio;
using Iot.Device.DHTxx;
using SensorPi.Models;

namespace SensorPi.Accessories.Temperature;

public sealed class SimpleTemperatureSensor(int pin, GpioController? gpioController = null) : ITemperatureSensor
{
    private static readonly TimeSpan ReadDelay = TimeSpan.FromSeconds(1);

    private readonly Dht11 _dht = new(pin, PinNumberingScheme.Logical, gpioController, gpioController == null)
    {
        MinTimeBetweenReads = ReadDelay,
    };
   
    private DateTime _lastRead = DateTime.MinValue;

    ~SimpleTemperatureSensor() => Dispose(false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposing) return;
        _dht.Dispose();
    }

    private TemperatureValues? ReadCurrentSync() {
        var now = DateTime.Now;
        lock(_dht) {
            if (now.Subtract(_lastRead) < ReadDelay)
                return null;
            _lastRead = now;
            if (_dht.TryReadTemperature(out var temp) && _dht.TryReadHumidity(out var humidity))
                return new TemperatureValues(now, temp, humidity, null);
        }
        return null;
    }

    public async ValueTask<TemperatureValues?> ReadCurrent()
        => await Task.Run(ReadCurrentSync);
}
