using System.Device.Gpio;
using SensorPi.Accessories.Base;
using SensorPi.Models;

namespace SensorPi.Accessories;

public sealed class StatusLight : BaseGpioAccessory {
    private readonly int _redPin, _greenPin, _bluePin;

    private LedColor _lastColor = LedColor.Off;
    
    public StatusLight(int redPin, int greenPin, int bluePin, GpioController? gpio, bool disposeGpioController = false) : base(gpio, disposeGpioController) {
        (_redPin, _greenPin, _bluePin) = (redPin, greenPin, bluePin);
        Gpio.OpenPin(_redPin, PinMode.Output);
        Gpio.OpenPin(_greenPin, PinMode.Output);
        Gpio.OpenPin(_bluePin, PinMode.Output);
        SetColor(_lastColor);
    }

    protected override void Dispose(bool disposing) {
        if (!disposing) return;
        Gpio.ClosePin(_redPin);
        Gpio.ClosePin(_greenPin);
        Gpio.ClosePin(_bluePin);
    }

    public void SetColor(LedColor color) {
        Gpio.Write(_redPin, color.HasRed() ? PinValue.High : PinValue.Low);
        Gpio.Write(_greenPin, color.HasGreen() ? PinValue.High : PinValue.Low);
        Gpio.Write(_bluePin, color.HasBlue() ? PinValue.High : PinValue.Low);
        _lastColor = color;
    }

    public async Task Blink(LedColor color, int count = 3, TimeSpan? delay = null) {
        var actualDelay = delay ?? TimeSpan.FromSeconds(0.5);
        for (int i = 0; i < count; i++)
        {
            SetColor(color);
            await Task.Delay(actualDelay);
            SetColor(LedColor.Off);
            await Task.Delay(actualDelay);
        }
        SetColor(_lastColor);
    }
}
