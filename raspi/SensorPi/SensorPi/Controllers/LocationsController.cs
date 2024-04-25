using SensorPi.Accessories;

namespace SensorPi.Controllers;

public sealed class LocationsController(WideDisplay? display = null)
{
    private static readonly string[] Locations = [
        "Office Desk", 
        "Living Room", 
        "Bedroom",
    ];

    private readonly object locker = new();
    private int _currentLocationIndex = 0;
    private readonly WideDisplay? _display = display;

    public string GetCurrentLocation() {
        lock(locker) return Locations[_currentLocationIndex];
    } 

    public async Task SwitchCurrentLocationTo(int index) 
    {
        // Verify index is valid and set to _currentLocationIndex
        while (index < 0 || index >= Locations.Length) {
            if (index >=  Locations.Length) {
                index -=  Locations.Length;
            } else {
                index +=  Locations.Length;
            }
        }     
        lock(locker) {
            if (index == _currentLocationIndex) return;
            _currentLocationIndex = index;
        }
        if (_display != null)
            await _display.ShowActivityIndicator();
    }

    public async Task SwitchCurrentLocationBy(int diff) 
    {
        if (diff == 0) return;
        int currentIndex;
        lock(locker) currentIndex = _currentLocationIndex;
        await SwitchCurrentLocationTo(currentIndex + diff);
    }
}