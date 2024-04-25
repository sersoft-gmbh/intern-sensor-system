
using System.Device.Gpio;
using Iot.Device.DHTxx;
using SensorPi.Models;

namespace SensorPi.Accessories.Temperature;

public sealed class SimpleTemperatureSensor : ITemperatureSensor {
    private static readonly TimeSpan ReadDelay = TimeSpan.FromSeconds(1);

    private readonly Dht11 _dht;
   
    private DateTime lastRead = DateTime.MinValue;

    public SimpleTemperatureSensor(int pin, GpioController? gpioController = null)
    {
        _dht = new Dht11(pin, PinNumberingScheme.Logical, gpioController, gpioController == null)
        {
            MinTimeBetweenReads = ReadDelay,
        };
    }

    public void Dispose()
    {
        _dht.Dispose();
        GC.SuppressFinalize(this);
    }

    private TemperatureValues? ReadCurrentSync() {
        var now = DateTime.Now;
        lock(_dht) {
            if (now.Subtract(lastRead) < ReadDelay)
                return null;
            lastRead = now;
            if (_dht.TryReadTemperature(out var temp) && _dht.TryReadHumidity(out var humidity))
                return new TemperatureValues(now, temp, humidity);
        }
        return null;
    }

    public async ValueTask<TemperatureValues?> ReadCurrent()
    {
        return await Task.Run(ReadCurrentSync);
    }
}
