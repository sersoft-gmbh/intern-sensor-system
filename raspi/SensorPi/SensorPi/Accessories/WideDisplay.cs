using System.Device.I2c;
using Iot.Device.CharacterLcd;
using SensorPi.Models;

namespace SensorPi.Accessories;

public sealed class WideDisplay : IDisposable {
    private readonly record struct Position(int Left, int Top);

    private readonly I2cDevice _i2c;
    private readonly Lcd1602 _lcd;

    private readonly object _activityIndicatorLock = new();
    private Timer? _activityIndicatorTimer;

    private bool IsShowingActivityIndicator
    {
        get {
            lock(_activityIndicatorLock) { 
                return _activityIndicatorTimer != null; 
            }
        }
    }

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
        CreateCustomCharacters();
    }

    public void Dispose()
     {
        _activityIndicatorTimer?.Dispose();
        _lcd.Clear();
        _lcd.Dispose();
        _i2c.Dispose();
        GC.SuppressFinalize(this);
    }

    private void WriteTextAtPosition(Position pos, string text)
    {
        lock(_lcd) {
            _lcd.SetCursorPosition(pos.Left, pos.Top);
            _lcd.Write(text);
        }
    }

    private void WriteCustomCharacter(Position pos, int customCharacaterNumber)
    {
        lock(_lcd) {
            _lcd.SetCursorPosition(pos.Left, pos.Top);
            _lcd.Write([(char)customCharacaterNumber]);
        }
    }

    private void WriteTextAtLine(int line, string text) 
    {
        if (!IsOn) return;
        var fullLine = text;
        var maxWidth = _lcd.Size.Width;
        if (line == 0 && IsShowingActivityIndicator)
            maxWidth -= 1;
        if (text.Length < maxWidth)
            fullLine += new string(' ', maxWidth - text.Length);
        else
            fullLine = fullLine.Remove(maxWidth);
        WriteTextAtPosition(new Position(0, line), fullLine);
    }

    private int _activityIndicatorCustomCharNumber = 0;
    private void ActivityIndicatorTimerTick()
    {
        WriteCustomCharacter(new Position(_lcd.Size.Width - 1, 0), _activityIndicatorCustomCharNumber);
        if (_activityIndicatorCustomCharNumber == 7)
            _activityIndicatorCustomCharNumber = 0;
        else
            _activityIndicatorCustomCharNumber += 1;
    }

    public async Task ShowActivityIndicator() 
    {
        await Task.Run(() => {
            lock(_activityIndicatorLock) {
                if (_activityIndicatorTimer != null) return;
                _activityIndicatorTimer = new Timer(_ => ActivityIndicatorTimerTick(), null, TimeSpan.Zero, TimeSpan.FromSeconds(0.25));
            }
        });
    }

    public async Task HideActivityIndicator() 
    {
        await Task.Run(() => {
            lock(_activityIndicatorLock) {
                if (_activityIndicatorTimer == null) return;
                _activityIndicatorTimer?.Dispose();
                _activityIndicatorTimer = null;
                WriteTextAtPosition(new Position(_lcd.Size.Width - 1, 0), " ");
            }
        });
    }

    public async Task WriteMeasurement(Measurement measurement) 
    {
        await HideActivityIndicator();
        await Task.Run(() => {
            WriteTextAtLine(0, measurement.Location);
            string measurementLine;
            if (measurement.PressureHectopascals is {} pressure)
                measurementLine = $"{measurement.TemperatureCelsius:N0}C  {measurement.HumidityPercent:P0}  {pressure:N0}hP";
            else
                measurementLine = $"{measurement.TemperatureCelsius:N2}C | {measurement.HumidityPercent:P2}";
            WriteTextAtLine(1, measurementLine);
        });
    }

    private void CreateCustomCharacters()
    {
        _lcd.CreateCustomCharacter(0, [
            0b00000,
            0b01110,
            0b10000,
            0b10000,
            0b10001,
            0b10001,
            0b01110,
            0b00000,
        ]);
        _lcd.CreateCustomCharacter(1, [
            0b00000,
            0b01110,
            0b10001,
            0b10001,
            0b10000,
            0b10000,
            0b01110,
            0b00000,
        ]);
        _lcd.CreateCustomCharacter(2, [
            0b00000,
            0b01110,
            0b10001,
            0b10001,
            0b10001,
            0b10001,
            0b01000,
            0b00000,
        ]);
        _lcd.CreateCustomCharacter(3, [
            0b00000,
            0b01110,
            0b10001,
            0b10001,
            0b10001,
            0b00001,
            0b00110,
            0b00000,
        ]);
        _lcd.CreateCustomCharacter(4, [
            0b00000,
            0b01110,
            0b10001,
            0b00001,
            0b00001,
            0b10001,
            0b01110,
            0b00000,
        ]);
        _lcd.CreateCustomCharacter(5, [
            0b00000,
            0b00110,
            0b00001,
            0b10001,
            0b10001,
            0b10001,
            0b01110,
            0b00000,
        ]);
        _lcd.CreateCustomCharacter(6, [
            0b00000,
            0b01000,
            0b10001,
            0b10001,
            0b10001,
            0b10001,
            0b01110,
            0b00000,
        ]);
        _lcd.CreateCustomCharacter(7, [
            0b00000,
            0b01100,
            0b10000,
            0b10001,
            0b10001,
            0b10001,
            0b01110,
            0b00000,
        ]);
    }
}
