using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SensorServer.Models;

namespace SensorServer.Repositories;

public class MeasurementsRepository: DbContext
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

    public Measurement[] AllMeasurements(int maxLength, DateTime? start = null, DateTime? stop = null, string? location = null)
    {
        return Measurements
            .Where(m => start == null || m.Date >= start)
            .Where(m => stop == null || m.Date <= stop)
            .Where(m => location == null || m.Location == location)
            .OrderByDescending(m => m.Date)
            .Take(maxLength)
            .ToArray();
    }

    public Measurement? GetMeasurement(long id)
    {
        return Measurements.Find(id);
    }

    private Measurement? GetFirstMeasurement<TKey>(bool max,Expression<Func<Measurement, TKey>> keySelector, string? location = null)
    {
        var filtered = Measurements.Where(m => location == null || m.Location == location);
        return max ? filtered.OrderByDescending(keySelector).FirstOrDefault() : filtered.OrderBy(keySelector).FirstOrDefault();
    }
    
    public Measurement? GetMaxMeasurementBy<TKey>(Expression<Func<Measurement, TKey>> keySelector, string? location)
    {
        return GetFirstMeasurement(true, keySelector, location);
    }
    
    public Measurement? GetMinMeasurementBy<TKey>(Expression<Func<Measurement, TKey>> keySelector, string? location)
    {
        return GetFirstMeasurement(false, keySelector, location);
    }

    public string[] AllLocations()
    {
        return Measurements.Select(m => m.Location).Distinct().ToArray();
    }

    public Measurement Add(Measurement measurement)
    {
        var result = Measurements.Add(measurement);
        SaveChanges();
        return result.Entity;
    }
}