using System.Device.Gpio;
using SensorPi.Accessories.Base;
using SensorPi.Controllers;

namespace SensorPi.Accessories.Buttons;

public sealed class NightModeButton : BaseGpioAccessory {
    private readonly int _pin;

    private readonly NightModeController _nightModeController;

    public NightModeButton(int pin,
        NightModeController nightModeController,
        GpioController? gpio = null,
        bool disposeGpioController = false) : base(gpio, disposeGpioController) {
        _pin = pin;
        _nightModeController = nightModeController;
        Gpio.OpenPin(_pin, PinMode.Input);
        Gpio.RegisterCallbackForPinValueChangedEvent(_pin, PinEventTypes.Rising, async (o, a) => await PinValueChanged(o, a));
    }

    protected override void Dispose(bool disposing) 
    {
        if (!disposing) return;
        Gpio.UnregisterCallbackForPinValueChangedEvent(_pin, async (o, a) => await PinValueChanged(o, a));
        Gpio.ClosePin(_pin);
    }

    private async Task PinValueChanged(object sender, PinValueChangedEventArgs args) 
    {
        await _nightModeController.ToggleNightMode();
    }
}
