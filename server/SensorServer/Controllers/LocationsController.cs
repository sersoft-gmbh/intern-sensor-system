using Microsoft.AspNetCore.Mvc;
using SensorServer.Repositories;

namespace SensorServer.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class LocationsController : ControllerBase
{
    private readonly MeasurementsRepository _measurementsRepository;
    private readonly ILogger<LocationsController> _logger;

    public LocationsController(MeasurementsRepository measurementsRepository, ILogger<LocationsController> logger)
    {
        _measurementsRepository = measurementsRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<string[]> GetLocations()
    {
        return await _measurementsRepository.AllLocations();
    }
}
