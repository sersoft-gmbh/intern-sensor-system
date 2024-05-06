namespace SensorServer.Helpers;

public static class TemperatureConverter
{
    public static double ToFahrenheit(this double celsius) => celsius * 1.8 + 32;
    public static double ToCelsius(this double fahrenheit) => (fahrenheit - 32) * 0.55555;
}
