using System.Device.I2c;
using Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.ReadResult;
using SensorPi.Models;

namespace SensorPi.Accessories.Temperature;

public sealed class AdvancedTemperatureSensor : ITemperatureSensor 
{
    private readonly I2cDevice _i2c;
    private readonly Bme680 _bme;

    public AdvancedTemperatureSensor(int bus, int address) {
        _i2c = I2cDevice.Create(new I2cConnectionSettings(bus, address));
        _bme = new Bme680(_i2c);
    }

    ~AdvancedTemperatureSensor() => Dispose(false);

    public void Dispose() 
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposing) return;
        _bme.Dispose();
        _i2c.Dispose();
    }

    public async ValueTask<TemperatureValues?> ReadCurrent()
    {
        var current = await _bme.ReadAsync();
        if (current.Temperature is null || current.Humidity is null)
            return null;
        return new TemperatureValues(DateTime.Now, current.Temperature.Value, current.Humidity.Value, current.Pressure);
    }
}
