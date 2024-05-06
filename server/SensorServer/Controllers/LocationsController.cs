using Microsoft.AspNetCore.Mvc;
using SensorServer.Repositories;

namespace SensorServer.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class LocationsController(MeasurementsRepository measurementsRepository, ILogger<LocationsController> logger) : ControllerBase
{
    private readonly ILogger<LocationsController> _logger = logger;

    [HttpGet]
    public IAsyncEnumerable<string> GetLocations() => measurementsRepository.AllLocations();
}
