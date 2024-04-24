using System.Device.Gpio;

namespace SensorPi.Accessories.Base;

public abstract class BaseGpioAccessory(GpioController? gpio, bool disposeGpioController) : IDisposable {
    private readonly GpioController _gpio = gpio ?? new GpioController();
    private readonly bool _disposeGpioController = gpio == null || disposeGpioController;

    protected GpioController Gpio => _gpio;

    public void Dispose() {
        Dispose(true);
        if (_disposeGpioController)
            _gpio.Dispose();
        GC.SuppressFinalize(this);
    }

    protected abstract void Dispose(bool disposing);
}
