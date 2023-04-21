using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SensorServer.Models;
using SensorServer.Repositories;

namespace SensorServer.Controllers;

[ApiController]
[Route("[controller]")]
public class MeasurementsController : ControllerBase
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
        [FromQuery] SortDirection? sortDirection,
        [FromQuery] int? count,
        [FromQuery] int? skip,
        [FromQuery] string? location,
        [FromQuery] DateTime? start,
        [FromQuery] DateTime? stop)
    {
        return _measurementsRepository.AllMeasurements(
            sortDirection ?? SortDirection.Descending,
            count ?? 100,
            skip ?? 0,
            location,
            start,
            stop);
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
        return ActionResultFor(_measurementsRepository.GetLatestMeasurement(location));
    }

    [HttpGet]
    [Route("statistics")]
    public MeasurementStatistics GetMeasurementStatistics([FromQuery] string? location, [FromQuery] DateTime? start,
        [FromQuery] DateTime? stop)
    {
        return _measurementsRepository.GetMeasurementStatistics(location, start, stop);
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
