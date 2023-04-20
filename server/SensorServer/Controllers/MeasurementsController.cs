using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SensorServer.Models;
using SensorServer.Repositories;

namespace SensorServer.Controllers;

[ApiController]
[Route("[controller]")]
public class MeasurementsController: ControllerBase
{
    private readonly MeasurementsRepository _measurementsRepository;
    private readonly ILogger<MeasurementsController> _logger;

    public MeasurementsController(
        MeasurementsRepository measurementsRepository,
        ILogger<MeasurementsController> logger)
    {
        _measurementsRepository = measurementsRepository;
        _logger = logger;
    }

    [HttpGet]
    public Measurement[] GetMeasurements(
        [FromQuery] int? count,
        [FromQuery] int? skip,
        [FromQuery] string? location,
        [FromQuery] DateTime? start,
        [FromQuery] DateTime? stop)
    {
        return _measurementsRepository.AllMeasurements(count ?? 100, skip ?? 0, location, start, stop);
    }
    
    [HttpGet]
    [Route("{id:long}")]
    public ActionResult<Measurement> GetMeasurements([FromRoute] long id)
    {
        return ActionResultFor(_measurementsRepository.GetMeasurement(id));
    }

    [HttpGet]
    [Route("latest")]
    public ActionResult<Measurement> GetLatestMeasurement([FromQuery] string? location)
    {
        return ActionResultFor(_measurementsRepository.GetMaxMeasurementBy(m => m.Date, location));
    }
    
    // [HttpGet]
    // [Route("statistics")]
    // TODO: Add method GetMeasurementStatistics
    // Minimum required arguments: [FromQuery] string? location

    [HttpGet]
    [Route("min-temperature")]
    public ActionResult<Measurement> GetLowestTemperature([FromQuery] string? location)
    {
        return ActionResultFor(_measurementsRepository.GetMinMeasurementBy(m => m.TemperatureCelsius, location));
    }
    
    [HttpGet]
    [Route("max-temperature")]
    public ActionResult<Measurement> GetGreatestTemperature([FromQuery] string? location)
    {
        return ActionResultFor(_measurementsRepository.GetMaxMeasurementBy(m => m.TemperatureCelsius, location));
    }
    
    [HttpGet]
    [Route("min-humidity")]
    public ActionResult<Measurement> GetLowestHumidity([FromQuery] string? location)
    {
        return ActionResultFor(_measurementsRepository.GetMinMeasurementBy(m => m.HumidityPercent, location));
    }
    
    [HttpGet]
    [Route("max-humidity")]
    public ActionResult<Measurement> GetGreatestHumidity([FromQuery] string? location)
    {
        return ActionResultFor(_measurementsRepository.GetMaxMeasurementBy(m => m.HumidityPercent, location));
    }
    
    [HttpPut]
    [Authorize]
    public Measurement PutMeasurement([FromBody] Measurement measurement)
    {
        return _measurementsRepository.Add(measurement);
    }
    
    #region Helpers
    private ActionResult<Measurement> ActionResultFor(Measurement? measurement)
    {
        if (measurement == null) return NotFound();
        return measurement;
    }
    #endregion
}