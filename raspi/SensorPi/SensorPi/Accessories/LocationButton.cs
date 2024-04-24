using System.Device.Gpio;
using SensorPi.Accessories.Base;
using SensorPi.Controllers;

namespace SensorPi.Accessories;

public sealed class LocationButton : BaseGpioAccessory {
    private readonly int _pin;

    private readonly LocationsController _locationsController;
    
    public LocationButton(int pin, 
        LocationsController locationsController,
        GpioController? gpio = null,
        bool disposeGpioController = false) : base(gpio, disposeGpioController) {
        _pin = pin;
        _locationsController = locationsController;
        Gpio.OpenPin(_pin, PinMode.Input);
        Gpio.RegisterCallbackForPinValueChangedEvent(_pin, PinEventTypes.Falling, PinValueChanged);
    }

    protected override void Dispose(bool disposing) {
        if (!disposing) return;
        Gpio.UnregisterCallbackForPinValueChangedEvent(_pin, PinValueChanged);
        Gpio.ClosePin(_pin);
    }

    private void PinValueChanged(object sender, PinValueChangedEventArgs args) {
        Task.Run(async () => await _locationsController.SwitchCurrentLocationBy(1));
    }
}
