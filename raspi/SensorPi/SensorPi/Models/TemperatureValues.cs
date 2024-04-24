namespace SensorPi.Models;

public readonly record struct TemperatureValues(
    DateTime Date,
    UnitsNet.Temperature Temperature, 
    UnitsNet.RelativeHumidity Humidity);