using System.Device.Gpio;
using SensorPi.Accessories.Base;
using SensorPi.Models;

namespace SensorPi.Accessories;

public sealed class StatusLight : BaseGpioAccessory {
    private readonly int _redPin, _greenPin, _bluePin;

    private readonly Lock _lock = new();
    private bool _isOff;
    
    public bool IsOn 
    {
        get => !_isOff;
        set {
            lock(_lock) {
                _isOff = !value;
            }
            if (!value)
                WriteColor(LedColor.Off);
        }
    }
    
    public StatusLight(int redPin, int greenPin, int bluePin, GpioController? gpio, bool disposeGpioController = false) : base(gpio, disposeGpioController)
     {
        (_redPin, _greenPin, _bluePin) = (redPin, greenPin, bluePin);
        Gpio.OpenPin(_redPin, PinMode.Output);
        Gpio.OpenPin(_greenPin, PinMode.Output);
        Gpio.OpenPin(_bluePin, PinMode.Output);
        WriteColor(LedColor.Off);
    }

    protected override void Dispose(bool disposing)
     {
        if (!disposing) return;
        Gpio.ClosePin(_redPin);
        Gpio.ClosePin(_greenPin);
        Gpio.ClosePin(_bluePin);
    }

    private void WriteColor(LedColor color) 
    {
        lock(_lock) {
            Gpio.Write(_redPin, color.HasRed() ? PinValue.High : PinValue.Low);
            Gpio.Write(_greenPin, color.HasGreen() ? PinValue.High : PinValue.Low);
            Gpio.Write(_bluePin, color.HasBlue() ? PinValue.High : PinValue.Low);
        }
    }

    public async Task SetColor(LedColor color) 
    {
        if (_isOff) return;
        await Task.Run(() => WriteColor(color));
    }
}
