using System.ComponentModel;
using System.Net.Mime;
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
    [ProducesResponseType<Measurement[]>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [EndpointSummary("Returns a list of measurements.")]
    public IAsyncEnumerable<Measurement> GetMeasurements(
        [FromQuery, Description("The direction in which to sort")] SortDirection sortDirection = SortDirection.Descending,
        [FromQuery, Description("The number of measurements to return")] int count = 100,
        [FromQuery, Description("The number of measurements to skip")] int skip = 0,
        [FromQuery, Description("Only include measurements from this location")] string? location = null,
        [FromQuery, Description("Only include measurements at or after this date")] DateTime? start = null,
        [FromQuery, Description("Only include measurements at or before this date")] DateTime? stop = null)
        => measurementsRepository.AllMeasurements(sortDirection, count, skip, location, start, stop);

    [HttpGet]
    [Route("{id:long}")]
    [ProducesResponseType<Measurement>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.Json)]
    [EndpointSummary("Returns the measurement with the given id.")]
    public async Task<ActionResult<Measurement>> GetMeasurements(
        [FromRoute, Description("The id of the measurement to return")] long id,
        CancellationToken cancellationToken)
        => ActionResultFor(await measurementsRepository.GetMeasurement(id, cancellationToken));

    [HttpGet]
    [Route("latest")]
    [ProducesResponseType<Measurement>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [EndpointSummary("Returns the latest measurement.")]
    public async Task<ActionResult<Measurement>> GetLatestMeasurement(
        [FromQuery, Description("Only include measurements from this location")] string? location,
        CancellationToken cancellationToken)
        => ActionResultFor(await measurementsRepository.GetLatestMeasurement(location, cancellationToken));

    [HttpGet]
    [Route("counts")]
    [ProducesResponseType<MeasurementCounts>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [EndpointSummary("Returns the current measurement counts.")]
    public async Task<MeasurementCounts> GetMeasurementCounts(
        [FromQuery, Description("Only include measurements at or after this date")] DateTime? start,
        [FromQuery, Description("Only include measurements at or before this date")] DateTime? stop,
        CancellationToken cancellationToken) =>
        await measurementsRepository.GetMeasurementCounts(start, stop, cancellationToken);

    [HttpGet]
    [Route("statistics")]
    [ProducesResponseType<MeasurementStatistics>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [EndpointSummary("Returns the current measurement statistics.")]
    public async Task<MeasurementStatistics> GetMeasurementStatistics(
        [FromQuery, Description("Only include measurements from this location")] string? location,
        [FromQuery, Description("Only include measurements at or after this date")] DateTime? start,
        [FromQuery, Description("Only include measurements at or before this date")] DateTime? stop,
        CancellationToken cancellationToken) =>
        await measurementsRepository.GetMeasurementStatistics(location, start, stop, cancellationToken);

    [HttpPut]
    [Authorize]
    [Consumes(typeof(Measurement), MediaTypeNames.Application.Json)]
    [ProducesResponseType<Measurement>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [EndpointSummary("Creates a new measurement.")]
    public async Task<Measurement> PutMeasurement([FromBody] Measurement measurement, CancellationToken cancellationToken)
        => await measurementsRepository.Add(measurement, cancellationToken);

    #region Helpers
    [NonAction]
    private ActionResult<Measurement> ActionResultFor(Measurement? measurement)
    {
        if (measurement == null) return NotFound();
        return measurement;
    }
    #endregion
}
