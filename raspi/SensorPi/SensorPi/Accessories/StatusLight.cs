using System.Device.Gpio;
using SensorPi.Accessories.Base;
using SensorPi.Models;

namespace SensorPi.Accessories;

public sealed class StatusLight : BaseGpioAccessory {
    private readonly struct Change(Func<Task> work)
    {
        internal readonly Guid Id = Guid.NewGuid();
        internal readonly Func<Task> Work = work;

        internal Change(Action voidWork) : this(() => { voidWork(); return Task.CompletedTask; })
        {
        }
    }

    private readonly int _redPin, _greenPin, _bluePin;

    private bool _isOff;
    private LedColor _lastColor = LedColor.Off;

    private readonly Queue<Change> _changeQueue = new();

    public bool IsOn {
        get => !_isOff;
        set {
            _isOff = !value;
            WriteColor(_isOff ? LedColor.Off : _lastColor);
        }
    }
    
    public StatusLight(int redPin, int greenPin, int bluePin, GpioController? gpio, bool disposeGpioController = false) : base(gpio, disposeGpioController) {
        (_redPin, _greenPin, _bluePin) = (redPin, greenPin, bluePin);
        Gpio.OpenPin(_redPin, PinMode.Output);
        Gpio.OpenPin(_greenPin, PinMode.Output);
        Gpio.OpenPin(_bluePin, PinMode.Output);
        WriteColor(_lastColor);
    }

    protected override void Dispose(bool disposing) {
        if (!disposing) return;
        Gpio.ClosePin(_redPin);
        Gpio.ClosePin(_greenPin);
        Gpio.ClosePin(_bluePin);
    }

    private void WriteColor(LedColor color) {
        Gpio.Write(_redPin, color.HasRed() ? PinValue.High : PinValue.Low);
        Gpio.Write(_greenPin, color.HasGreen() ? PinValue.High : PinValue.Low);
        Gpio.Write(_bluePin, color.HasBlue() ? PinValue.High : PinValue.Low);
    }

    private async Task PerformChange(Change change) 
    {
        _changeQueue.Enqueue(change);
        while (_changeQueue.TryDequeue(out var nextChange)) {
            await nextChange.Work();
            if (nextChange.Id == change.Id)
                break;
        }
    }

    public async Task SetColor(LedColor color) {
        if (_isOff) return;
        var change = new Change(() => WriteColor(color));
        await PerformChange(change);
        _lastColor = color;
    }

    public async Task Blink(LedColor color, int count = 3, TimeSpan? delay = null) {
        var actualDelay = delay ?? TimeSpan.FromSeconds(0.5);
        var change = new Change(async () => {
            for (int i = 0; i < count; i++)
            {
                await SetColor(color);
                await Task.Delay(actualDelay);
                await SetColor(LedColor.Off);
                await Task.Delay(actualDelay);
            }
            await SetColor(_lastColor);
        });
        await PerformChange(change);
    }    
}
