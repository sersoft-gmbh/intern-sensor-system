namespace SensorServer.Helpers;

public static class TemperatureConverter
{
    public static double ToFahrenheit(this double celsius)
    {
        return celsius * 1.8 + 32;
    }
    
    public static double ToCelsius(this double fahrenheit)
    {
        return (fahrenheit - 32) * 0.55555;
    }
}