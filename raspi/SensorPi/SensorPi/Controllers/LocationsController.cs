using SensorPi.Accessories;
using SensorPi.Models;

namespace SensorPi.Controllers;

public sealed class LocationsController
{
    private static readonly string[] Locations = [
        "Office Desk", 
        "Living Room", 
        "Bedroom",
    ];

    private int _currentLocationIndex = 0;

    private readonly StatusLight? _statusLight;
    private readonly WideDisplay? _display;

    public LocationsController(StatusLight? statusLight = null, WideDisplay? display = null)
    {
        _statusLight = statusLight;
        _display = display;
        _display?.WriteLocation(GetCurrentLocation());
    }

    public string GetCurrentLocation() => Locations[_currentLocationIndex];

    public async Task SwitchCurrentLocationTo(int index) {
        // Verify index is valid and set to _currentLocationIndex
        while (index < 0 || index >= Locations.Length) {
            if (index >=  Locations.Length) {
                index -=  Locations.Length;
            } else {
                index +=  Locations.Length;
            }
        }     
        if (index == _currentLocationIndex) return;
        _currentLocationIndex = index;
        _display?.WriteLocation(GetCurrentLocation());
        if (_statusLight != null)
            await _statusLight.Blink(LedColor.Red.WithGreen().WithBlue());
    }

    public async Task SwitchCurrentLocationBy(int diff) {
        if (diff == 0) return;
        await SwitchCurrentLocationTo(_currentLocationIndex + diff);
    }
}