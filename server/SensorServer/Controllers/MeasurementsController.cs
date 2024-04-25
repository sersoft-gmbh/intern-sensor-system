using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SensorServer.Models;
using SensorServer.Repositories;

namespace SensorServer.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class MeasurementsController(
    MeasurementsRepository measurementsRepository,
    ILogger<MeasurementsController> logger)
    : ControllerBase
{
    private readonly ILogger<MeasurementsController> _logger = logger;

    [HttpGet]
    public async Task<Measurement[]> GetMeasurements(
        [FromQuery] SortDirection? sortDirection,
        [FromQuery] int? count,
        [FromQuery] int? skip,
        [FromQuery] string? location,
        [FromQuery] DateTime? start,
        [FromQuery] DateTime? stop)
    {
        return await measurementsRepository.AllMeasurements(
            sortDirection ?? SortDirection.Descending,
            count ?? 100,
            skip ?? 0,
            location,
            start,
            stop);
    }

    [HttpGet]
    [Route("{id:long}")]
    public async Task<ActionResult<Measurement>> GetMeasurements([FromRoute] long id)
    {
        return ActionResultFor(await measurementsRepository.GetMeasurement(id));
    }

    [HttpGet]
    [Route("latest")]
    public async Task<ActionResult<Measurement>> GetLatestMeasurement([FromQuery] string? location)
    {
        return ActionResultFor(await measurementsRepository.GetLatestMeasurement(location));
    }

    [HttpGet]
    [Route("counts")]
    public async Task<MeasurementCounts> GetMeasurementCounts(
        [FromQuery] DateTime? start,
        [FromQuery] DateTime? stop)
    {
        return await measurementsRepository.GetMeasurementCounts(start, stop);
    }

    [HttpGet]
    [Route("statistics")]
    public async Task<MeasurementStatistics> GetMeasurementStatistics(
        [FromQuery] string? location,
        [FromQuery] DateTime? start,
        [FromQuery] DateTime? stop)
    {
        return await measurementsRepository.GetMeasurementStatistics(location, start, stop);
    }

    [HttpPut]
    [Authorize]
    public async Task<Measurement> PutMeasurement([FromBody] Measurement measurement)
    {
        return await measurementsRepository.Add(measurement);
    }

    #region Helpers
    [NonAction]
    private ActionResult<Measurement> ActionResultFor(Measurement? measurement)
    {
        if (measurement == null) return NotFound();
        return measurement;
    }
    #endregion
}
