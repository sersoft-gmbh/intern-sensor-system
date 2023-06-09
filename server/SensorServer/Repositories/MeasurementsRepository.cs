using Microsoft.EntityFrameworkCore;
using SensorServer.Models;

namespace SensorServer.Repositories;

public class MeasurementsRepository : DbContext
{
    private static ILoggerFactory ContextLoggerFactory
        => LoggerFactory.Create(b => b.AddConsole().AddFilter("", LogLevel.Information));
    private static bool _didLogConnectionString;

    private readonly IConfiguration _configuration;
    private readonly ILogger<MeasurementsRepository> _logger;

    public DbSet<Measurement> Measurements { get; set; } = null!;

    public MeasurementsRepository(IConfiguration configuration, ILogger<MeasurementsRepository> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (!_didLogConnectionString)
        {
            _logger.LogInformation("Using database connection string: {ConnectionString}", connectionString);
            _didLogConnectionString = true;
        }

        optionsBuilder.UseSqlite(connectionString).UseLoggerFactory(ContextLoggerFactory);
    }

    public async Task<string[]> AllLocations()
    {
        return await Measurements.TagWith(nameof(AllLocations)).Select(m => m.Location).Distinct().ToArrayAsync();
    }

    public async Task<Measurement[]> AllMeasurements(
        SortDirection sortDirection,
        int count,
        int skip = 0,
        string? location = null,
        DateTime? start = null,
        DateTime? stop = null)
    {
        var filtered = FilteredMeasurements(location, start, stop).TagWith(nameof(AllMeasurements));
        switch (sortDirection)
        {
            case SortDirection.Ascending:
                filtered = filtered.OrderBy(m => m.Date);
                break;
            case SortDirection.Descending:
                filtered = filtered.OrderByDescending(m => m.Date);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(sortDirection), sortDirection, null);
        }
        return await filtered
            .Skip(skip)
            .Take(count)
            .ToArrayAsync();
    }

    public async Task<Measurement?> GetMeasurement(long id)
    {
        return await Measurements.FindAsync(id);
    }

    public async Task<Measurement?> GetLatestMeasurement(string? location)
    {
        return await FilteredMeasurements(location, null, null)
            .TagWith(nameof(GetLatestMeasurement))
            .OrderByDescending(m => m.Date)
            .FirstOrDefaultAsync();
    }

    public async Task<MeasurementCounts> GetMeasurementCounts(DateTime? start = null, DateTime? stop = null)
    {
        var measurements = FilteredMeasurements(null, start, stop)
            .TagWith(nameof(GetMeasurementCounts));
        var total = measurements
            .TagWith("Total")
            .LongCountAsync();
        var perLocation = measurements
            .TagWith("Per Location")
            .GroupBy(m => m.Location)
            .Select(x => new {Location = x.Key, Count = x.LongCount()})
            .ToDictionaryAsync(g => g.Location, g => g.Count);
        return new MeasurementCounts
        {
            Total = await total,
            PerLocation = await perLocation,
        };
    }

    public async Task<MeasurementStatistics> GetMeasurementStatistics(
        string? location = null,
        DateTime? start = null,
        DateTime? stop = null)
    {
        var filtered = FilteredMeasurements(location, start, stop)
            .TagWith(nameof(GetMeasurementStatistics));
        var count = await filtered.TagWith("Total").LongCountAsync();
        if (count == 0) return new MeasurementStatistics();
        var medianSkip = (int)(count / 2);
        var averageTemperatureCelsius = filtered.TagWith("Average Temperature").AverageAsync(m => m.TemperatureCelsius);
        var averageHumidityPercent = filtered.TagWith("Average Humidity").AverageAsync(m => m.HumidityPercent);
        var minTemperature = filtered.TagWith("Min Temperature").OrderBy(m => m.TemperatureCelsius).FirstOrDefaultAsync();
        var maxTemperature = filtered.TagWith("Max Temperature").OrderByDescending(m => m.TemperatureCelsius).FirstOrDefaultAsync();
        var minHumidity = filtered.TagWith("Min Humidity").OrderBy(m => m.HumidityPercent).FirstOrDefaultAsync();
        var maxHumidity = filtered.TagWith("MaxHumidity").OrderByDescending(m => m.HumidityPercent).FirstOrDefaultAsync();
        var medianTemperature = filtered.TagWith("Median Temperature").OrderBy(m => m.TemperatureCelsius).Skip(medianSkip).FirstOrDefaultAsync();
        var medianHumidity = filtered.TagWith("Median Humidity").OrderBy(m => m.HumidityPercent).Skip(medianSkip).FirstOrDefaultAsync();
        return new MeasurementStatistics
        {
            AverageTemperatureCelsius = await averageTemperatureCelsius,
            AverageHumidityPercent = await averageHumidityPercent,
            MinTemperature = await minTemperature,
            MaxTemperature = await maxTemperature,
            MinHumidity = await minHumidity,
            MaxHumidity = await maxHumidity,
            MedianTemperature = await medianTemperature,
            MedianHumidity = await medianHumidity,
        };
    }

    public async Task<Measurement> Add(Measurement measurement)
    {
        var result = await Measurements.AddAsync(measurement);
        await SaveChangesAsync();
        return result.Entity;
    }

    #region Helpers

    private IQueryable<Measurement> FilteredMeasurements(string? location, DateTime? start, DateTime? stop)
    {
        IQueryable<Measurement> filtered = Measurements;
        if (location != null)
        {
            filtered = filtered.Where(m => m.Location == location);
        }

        if (start != null)
        {
            filtered = filtered.Where(m => m.Date >= start);
        }

        if (stop != null)
        {
            filtered = filtered.Where(m => m.Date <= stop);
        }

        return filtered;
    }

    #endregion
}
