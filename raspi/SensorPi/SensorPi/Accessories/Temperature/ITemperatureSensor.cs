using SensorPi.Models;

namespace SensorPi.Accessories.Temperature;

public interface ITemperatureSensor : IDisposable
{
    public ValueTask<TemperatureValues?> ReadCurrent();
}
