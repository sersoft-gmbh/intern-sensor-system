using System.Device.Gpio;

namespace SensorPi.Accessories.Base;

public abstract class BaseGpioAccessory(GpioController? gpio, bool disposeGpioController) : IDisposable {
    private readonly bool _disposeGpioController = gpio == null || disposeGpioController;

    protected GpioController Gpio { get; } = gpio ?? new GpioController();

    ~BaseGpioAccessory() => Dispose(false);

    public void Dispose() {
        Dispose(true);
        if (_disposeGpioController)
            Gpio.Dispose();
        GC.SuppressFinalize(this);
    }

    protected abstract void Dispose(bool disposing);
}
