using System.Device.Gpio;
using SensorPi.Accessories.Base;

namespace SensorPi.Accessories.Buttons;

public sealed class NightModeButton : BaseGpioAccessory {
    private readonly int _pin;

    private readonly StatusLight? _statusLight;
    private readonly WideDisplay? _display;

    public NightModeButton(int pin,
        StatusLight? statusLight = null, 
        WideDisplay? display = null,
        GpioController? gpio = null,
        bool disposeGpioController = false) : base(gpio, disposeGpioController) {
        _pin = pin;
        _statusLight = statusLight;
        _display = display;
        Gpio.OpenPin(_pin, PinMode.Input);
        Gpio.RegisterCallbackForPinValueChangedEvent(_pin, PinEventTypes.Rising, PinValueChanged);
    }

    protected override void Dispose(bool disposing) 
    {
        if (!disposing) return;
        Gpio.UnregisterCallbackForPinValueChangedEvent(_pin, PinValueChanged);
        Gpio.ClosePin(_pin);
    }

    public void ToggleNightMode()
     {
        if (_statusLight != null)
            _statusLight.IsOn = !_statusLight.IsOn;
        if (_display != null)
            _display.IsOn = !_display.IsOn;
    }

    public void PinValueChanged(object sender, PinValueChangedEventArgs args) 
    {
        ToggleNightMode();
    }
}
