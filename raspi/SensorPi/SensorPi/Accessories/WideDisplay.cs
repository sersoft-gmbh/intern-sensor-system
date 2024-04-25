using System.Device.I2c;
using Iot.Device.CharacterLcd;
using SensorPi.Models;

namespace SensorPi.Accessories;

public sealed class WideDisplay : IDisposable {
    private readonly record struct Position(int Left, int Top);

    private readonly I2cDevice _i2c;
    private readonly Lcd1602 _lcd;

    public bool IsOn
    {
        get => _lcd.DisplayOn;
        set
        {
            _lcd.DisplayOn = value;
            _lcd.BacklightOn = value;
        }
    }

    public WideDisplay(int bus, int address) 
    {
        _i2c = I2cDevice.Create(new I2cConnectionSettings(bus, address));
        _lcd = new Lcd1602(_i2c, false);
        _lcd.Clear();
    }

    public void Dispose()
     {
        _lcd.Clear();
        _lcd.Dispose();
        _i2c.Dispose();
        GC.SuppressFinalize(this);
    }

    private void WriteTextAtPosition(Position pos, string text)
     {
        _lcd.SetCursorPosition(pos.Left, pos.Top);
        _lcd.Write(text);
    }

    private void WriteTextAtLine(int line, string text) 
    {
        if (!IsOn) return;
        var fullLine = text;
        if (text.Length < _lcd.Size.Width)
            fullLine += new string(' ', _lcd.Size.Width - text.Length);
        WriteTextAtPosition(new Position(0, line), fullLine);
    }

    public void WriteLocation(string location) 
    {
        WriteTextAtLine(0, location);
    }

    public void WriteMeasurement(Measurement measurement) 
    {
        WriteLocation(measurement.Location);
        WriteTextAtLine(1, $"{measurement.TemperatureCelsius:N2}C | {measurement.HumidityPercent:P2}");
    }
}
