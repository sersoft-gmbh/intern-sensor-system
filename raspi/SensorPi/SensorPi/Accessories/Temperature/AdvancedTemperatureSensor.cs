using System.Device.I2c;
using Iot.Device.Bmxx80;
using SensorPi.Models;

namespace SensorPi.Accessories.Temperature;

public sealed class AdvancedTemperatureSensor : ITemperatureSensor 
{
    private readonly I2cDevice _i2c;
    private readonly Bme680 _bm680;

    public AdvancedTemperatureSensor(int bus, int address) {
        _i2c = I2cDevice.Create(new I2cConnectionSettings(bus, address));
        _bm680 = new Bme680(_i2c);
    }

    public void Dispose() 
    {
        _bm680.Dispose();
        _i2c.Dispose();
        GC.SuppressFinalize(this);
    }

    public async ValueTask<TemperatureValues?> ReadCurrent()
    {
        var current = await _bm680.ReadAsync();
        if (current == null || current.Temperature is null || current.Humidity is null) 
            return null;
        return new TemperatureValues(DateTime.Now, current.Temperature.Value, current.Humidity.Value);
    }
}
