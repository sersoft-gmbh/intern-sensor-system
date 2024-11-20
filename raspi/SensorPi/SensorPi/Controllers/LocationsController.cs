using SensorPi.Accessories;

namespace SensorPi.Controllers;

public sealed class LocationsController(WideDisplay? display = null)
{
    private static readonly string[] Locations = [
        "Office Desk", 
        "Living Room", 
        "Bedroom",
    ];

    private readonly Lock _lock = new();
    private int _currentLocationIndex;

    public string GetCurrentLocation() {
        lock(_lock) return Locations[_currentLocationIndex];
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
        lock(_lock) {
            if (index == _currentLocationIndex) return;
            _currentLocationIndex = index;
        }
        if (display != null)
            await display.ShowActivityIndicator();
    }

    public async Task SwitchCurrentLocationBy(int diff) 
    {
        if (diff == 0) return;
        int currentIndex;
        lock(_lock) currentIndex = _currentLocationIndex;
        await SwitchCurrentLocationTo(currentIndex + diff);
    }
}