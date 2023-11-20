using SensorServer.Models;

namespace SensorServer.Helpers;

public static class HeatIndexCalculator
{
    public static double CalculateHeatIndexInFahrenheit(this Measurement measurement)
    {
        var fahrenheit = measurement.TemperatureFahrenheit;
        var humidityPercent = measurement.HumidityPercent * 100;

        var heatIndex = 0.5 * (fahrenheit + 61 + (fahrenheit - 68) * 1.2 + humidityPercent * 0.094);

        if (heatIndex <= 79) return heatIndex;

        heatIndex = -42.379 + 2.04901523 * fahrenheit + 10.14333127 * humidityPercent +
                    -0.22475541 * fahrenheit * humidityPercent +
                    -0.00683783 * Math.Pow(fahrenheit, 2) +
                    -0.05481717 * Math.Pow(humidityPercent, 2) +
                    0.00122874 * Math.Pow(fahrenheit, 2) * humidityPercent +
                    0.00085282 * fahrenheit * Math.Pow(humidityPercent, 2) +
                    -0.00000199 * Math.Pow(fahrenheit, 2) * Math.Pow(humidityPercent, 2);

        switch (humidityPercent)
        {
            case < 13 when fahrenheit is >= 80 and <= 112:
                heatIndex -= (13 - humidityPercent) * 0.25 * Math.Sqrt((17 - Math.Abs(fahrenheit - 95)) * 0.05882);
                break;
            case > 85 when fahrenheit is >= 80 and <= 87:
                heatIndex += (humidityPercent - 85) * 0.1 * ((87 - fahrenheit) * 0.2);
                break;
        }

        return heatIndex;
    }
}
