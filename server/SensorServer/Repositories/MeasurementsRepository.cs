using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SensorServer.Models;

namespace SensorServer.Repositories;

public class MeasurementsRepository : DbContext
{
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
            _logger.LogInformation("Using database connection string: {connectionString}", connectionString);
            _didLogConnectionString = true;
        }

        optionsBuilder.UseSqlite(connectionString);
    }

    public string[] AllLocations()
    {
        return Measurements.Select(m => m.Location).Distinct().ToArray();
    }

    public Measurement[] AllMeasurements(
        SortDirection sortDirection,
        int count,
        int skip = 0,
        string? location = null,
        DateTime? start = null,
        DateTime? stop = null)
    {
        var filtered = FilteredMeasurements(location, start, stop);
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
        return filtered
            .Skip(skip)
            .Take(count)
            .ToArray();
    }

    public Measurement? GetMeasurement(long id)
    {
        return Measurements.Find(id);
    }

    public Measurement? GetLatestMeasurement(string? location)
    {
        return FilteredMeasurements(location, null, null).OrderByDescending(m => m.Date).FirstOrDefault();
    }

    public MeasurementStatistics GetMeasurementStatistics(
        string? location = null,
        DateTime? start = null,
        DateTime? stop = null)
    {
        var filtered = FilteredMeasurements(location, start, stop);
        var count = filtered.Count();
        return new MeasurementStatistics
        {
            AverageTemperatureCelsius = filtered.Average(m => m.TemperatureCelsius),
            AverageHumidityPercent = filtered.Average(m => m.HumidityPercent),
            MinTemperature = filtered.OrderBy(m => m.TemperatureCelsius).FirstOrDefault(),
            MaxTemperature = filtered.OrderByDescending(m => m.TemperatureCelsius).FirstOrDefault(),
            MinHumidity = filtered.OrderBy(m => m.HumidityPercent).FirstOrDefault(),
            MaxHumidity = filtered.OrderByDescending(m => m.HumidityPercent).FirstOrDefault(),
            MedianTemperature = filtered.OrderBy(m => m.TemperatureCelsius).Skip(count / 2).FirstOrDefault(),
            MedianHumidity = filtered.OrderBy(m => m.HumidityPercent).Skip(count / 2).FirstOrDefault(),
        };
    }

    public Measurement Add(Measurement measurement)
    {
        var result = Measurements.Add(measurement);
        SaveChanges();
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
