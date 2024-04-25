using SensorPi.Accessories;

namespace SensorPi.Controllers;

public sealed class NightModeController(WideDisplay? display, StatusLight? statusLight)
{
    private readonly object locker = new();
    private bool _isNightModeOn = false;

    private readonly WideDisplay? _display = display;
    private readonly StatusLight? _statusLight = statusLight;

    
    public bool IsNightModeOn { 
        get {
            lock(locker) return _isNightModeOn;
        }
    }

    public Task ToggleNightMode() 
    {
        bool isOnNew;
        lock(locker) {
            _isNightModeOn = !_isNightModeOn;
            isOnNew = _isNightModeOn;
        } 
        if (_display != null)
            _display.IsOn = !isOnNew;
        if (_statusLight != null)
            _statusLight.IsOn = !isOnNew;
        return Task.CompletedTask;
    }
}
