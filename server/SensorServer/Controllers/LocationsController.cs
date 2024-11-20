using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using SensorServer.Repositories;

namespace SensorServer.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class LocationsController(MeasurementsRepository measurementsRepository, ILogger<LocationsController> logger) : ControllerBase
{
    private readonly ILogger<LocationsController> _logger = logger;

    [HttpGet]
    [ProducesResponseType<string[]>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [EndpointSummary("Returns the list of locations.")]
    public IAsyncEnumerable<string> GetLocations() => measurementsRepository.AllLocations();
}
