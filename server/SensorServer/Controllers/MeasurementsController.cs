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
    public IAsyncEnumerable<Measurement> GetMeasurements(
        [FromQuery] SortDirection? sortDirection,
        [FromQuery] int? count,
        [FromQuery] int? skip,
        [FromQuery] string? location,
        [FromQuery] DateTime? start,
        [FromQuery] DateTime? stop)
        => measurementsRepository.AllMeasurements(
            sortDirection ?? SortDirection.Descending,
            count ?? 100,
            skip ?? 0,
            location,
            start,
            stop);

    [HttpGet]
    [Route("{id:long}")]
    public async Task<ActionResult<Measurement>> GetMeasurements([FromRoute] long id)
        => ActionResultFor(await measurementsRepository.GetMeasurement(id));

    [HttpGet]
    [Route("latest")]
    public async Task<ActionResult<Measurement>> GetLatestMeasurement([FromQuery] string? location)
        => ActionResultFor(await measurementsRepository.GetLatestMeasurement(location));

    [HttpGet]
    [Route("counts")]
    public async Task<MeasurementCounts> GetMeasurementCounts(
        [FromQuery] DateTime? start,
        [FromQuery] DateTime? stop) =>
        await measurementsRepository.GetMeasurementCounts(start, stop);

    [HttpGet]
    [Route("statistics")]
    public async Task<MeasurementStatistics> GetMeasurementStatistics(
        [FromQuery] string? location,
        [FromQuery] DateTime? start,
        [FromQuery] DateTime? stop) =>
        await measurementsRepository.GetMeasurementStatistics(location, start, stop);

    [HttpPut]
    [Authorize]
    public async Task<Measurement> PutMeasurement([FromBody] Measurement measurement)
        => await measurementsRepository.Add(measurement);

    #region Helpers
    [NonAction]
    private ActionResult<Measurement> ActionResultFor(Measurement? measurement)
    {
        if (measurement == null) return NotFound();
        return measurement;
    }
    #endregion
}
