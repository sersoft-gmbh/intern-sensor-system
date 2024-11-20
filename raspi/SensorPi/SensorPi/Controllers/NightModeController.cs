using SensorPi.Accessories;

namespace SensorPi.Controllers;

public sealed class NightModeController(WideDisplay? display, StatusLight? statusLight)
{
    private readonly Lock _lock = new();
    private bool _isNightModeOn;

    public bool IsNightModeOn { 
        get {
            lock(_lock) return _isNightModeOn;
        }
    }

    public Task ToggleNightMode() 
    {
        bool isOnNew;
        lock(_lock) {
            _isNightModeOn = !_isNightModeOn;
            isOnNew = _isNightModeOn;
        } 
        if (display != null)
            display.IsOn = !isOnNew;
        if (statusLight != null)
            statusLight.IsOn = !isOnNew;
        return Task.CompletedTask;
    }
}
