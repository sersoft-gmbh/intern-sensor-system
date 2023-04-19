using Microsoft.AspNetCore.Mvc;
using SensorServer.Repositories;

namespace SensorServer.Controllers;

[ApiController]
[Route("[controller]")]
public class LocationsController: ControllerBase
{
    private readonly MeasurementsRepository _measurementsRepository;
    private readonly ILogger<MeasurementsController> _logger;

    public LocationsController(MeasurementsRepository measurementsRepository, ILogger<MeasurementsController> logger)
    {
        _measurementsRepository = measurementsRepository;
        _logger = logger;
    }
    
    [HttpGet]
    public string[] GetLocations()
    {
        return _measurementsRepository.AllLocations();
    }
}