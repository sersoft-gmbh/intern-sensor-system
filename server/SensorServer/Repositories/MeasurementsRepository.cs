using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SensorServer.Models;

namespace SensorServer.Repositories;

public sealed class MeasurementsRepository(
    ApplicationDbContext dbContext,
    IDbContextFactory<ApplicationDbContext> dbContextFactory
)
{
    public IAsyncEnumerable<string> AllLocations()
        => dbContext.Measurements.TagWith(nameof(AllLocations)).Select(m => m.Location).Distinct().AsAsyncEnumerable();

    public IAsyncEnumerable<Measurement> AllMeasurements(
        SortDirection sortDirection,
        int count,
        int skip = 0,
        string? location = null,
        DateTime? start = null,
        DateTime? stop = null)
    {
        var filtered = FilteredMeasurements(dbContext.Measurements.AsNoTracking(), location, start, stop)
            .TagWith(nameof(AllMeasurements));
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

        return filtered.Skip(skip).Take(count).AsAsyncEnumerable();
    }

    public async Task<Measurement?> GetMeasurement(long id)
    {
        return await dbContext.Measurements.FindAsync(id);
    }

    public async Task<Measurement?> GetLatestMeasurement(string? location)
    {
        return await FilteredMeasurements(dbContext.Measurements.AsNoTracking(), location, null, null)
            .TagWith(nameof(GetLatestMeasurement))
            .OrderByDescending(m => m.Date)
            .FirstOrDefaultAsync();
    }

    public async Task<MeasurementCounts> GetMeasurementCounts(DateTime? start = null, DateTime? stop = null)
    {
        var measurements = FilteredMeasurements(dbContext.Measurements.AsNoTracking(), null, start, stop)
            .TagWith(nameof(GetMeasurementCounts));
        var total = await measurements
            .TagWith("Total")
            .LongCountAsync();
        var perLocation = await measurements
            .TagWith("Per Location")
            .GroupBy(m => m.Location)
            .Select(x => new { Location = x.Key, Count = x.LongCount() })
            .ToDictionaryAsync(g => g.Location, g => g.Count);
        return new MeasurementCounts
        {
            Total = total,
            PerLocation = perLocation,
        };
    }

    public async Task<MeasurementStatistics> GetMeasurementStatistics(
        string? location = null,
        DateTime? start = null,
        DateTime? stop = null)
    {
        long count, countWithPressure;
        await using (var context = await dbContextFactory.CreateDbContextAsync())
        {
            var filtered = FilteredMeasurements(context.Measurements.AsNoTracking(), location, start, stop)
                .TagWith(nameof(GetMeasurementStatistics));
            count = await filtered.TagWith("Total").LongCountAsync();
            if (count == 0) return new MeasurementStatistics();
            countWithPressure = await filtered.Where(m => m.PressureHectopascals != null).TagWith("Total with Pressure").LongCountAsync();
        }

        var averageTemperatureCelsius = AverageMeasurementField(m => m.TemperatureCelsius, "Temperature", location, start, stop);
        var averageHumidityPercent = AverageMeasurementField(m => m.HumidityPercent, "Humidity", location, start, stop);
        var averagePressureHectopascals = AverageMeasurementField(m => m.PressureHectopascals, "Pressure", location, start, stop);
        var minTemperature = StatisticsMeasurement(m => m.TemperatureCelsius, "Temperature", StatisticKind.Min, count, location, start, stop);
        var maxTemperature = StatisticsMeasurement(m => m.TemperatureCelsius, "Temperature", StatisticKind.Max, count, location, start, stop);
        var medianTemperature = StatisticsMeasurement(m => m.TemperatureCelsius, "Temperature", StatisticKind.Median, count, location, start, stop);
        var minHumidity = StatisticsMeasurement(m => m.HumidityPercent, "Humidity", StatisticKind.Min, count, location, start, stop);
        var maxHumidity = StatisticsMeasurement(m => m.HumidityPercent, "Humidity", StatisticKind.Max, count, location, start, stop);
        var medianHumidity = StatisticsMeasurement(m => m.HumidityPercent, "Humidity", StatisticKind.Median, count, location, start, stop);
        var minPressure = StatisticsMeasurement(m => m.PressureHectopascals, "Pressure", StatisticKind.Min, countWithPressure, location, start, stop, true);
        var maxPressure = StatisticsMeasurement(m => m.PressureHectopascals, "Pressure", StatisticKind.Max, countWithPressure, location, start, stop, true);
        var medianPressure = StatisticsMeasurement(m => m.PressureHectopascals, "Pressure", StatisticKind.Median, countWithPressure, location, start, stop, true);

        return new MeasurementStatistics
        {
            AverageTemperatureCelsius = await averageTemperatureCelsius,
            AverageHumidityPercent = await averageHumidityPercent,
            AveragePressureHectopascals = await averagePressureHectopascals,

            MinTemperature = await minTemperature,
            MedianTemperature = await medianTemperature,
            MaxTemperature = await maxTemperature,

            MinHumidity = await minHumidity,
            MedianHumidity = await medianHumidity,
            MaxHumidity = await maxHumidity,

            MinPressure = await minPressure,
            MedianPressure = await medianPressure,
            MaxPressure = await maxPressure,
        };
    }

    public async Task<Measurement> Add(Measurement measurement)
    {
        var result = await dbContext.Measurements.AddAsync(measurement);
        await dbContext.SaveChangesAsync();
        return result.Entity;
    }

    #region Helpers

    private static IQueryable<Measurement> FilteredMeasurements(IQueryable<Measurement> measurements, string? location, DateTime? start, DateTime? stop)
    {
        var filtered = measurements;
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

    private static IQueryable<TResult> FilterNullValues<TResult, TField>(IQueryable<TResult> queryable, Expression<Func<TResult, TField>> fieldSelector)
    {
        var nullCheck = Expression.NotEqual(fieldSelector.Body, Expression.Constant(null));
        return queryable.Where(Expression.Lambda<Func<TResult, bool>>(nullCheck, fieldSelector.Parameters));
    }

    private enum StatisticKind
    {
        Min,
        Max,
        Median,
    }
    private async Task<Measurement?> StatisticsMeasurement<TField>(
        Expression<Func<Measurement, TField>> fieldSelector,
        string fieldName,
        StatisticKind kind,
        long count,
        string? location,
        DateTime? start,
        DateTime? stop,
        bool filterForNull = false)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var queryable = FilteredMeasurements(context.Measurements.AsNoTracking(), location, start, stop)
            .TagWith($"{kind} {fieldName}");
        if (filterForNull) queryable = FilterNullValues(queryable, fieldSelector);
        return kind switch
        {
            StatisticKind.Min => await queryable.OrderBy(fieldSelector).ThenBy(m => m.Id).FirstOrDefaultAsync(),
            StatisticKind.Max => await queryable.OrderByDescending(fieldSelector).ThenByDescending(m => m.Id).FirstOrDefaultAsync(),
            StatisticKind.Median => await queryable.OrderBy(fieldSelector).Skip((int)(count / 2)).FirstOrDefaultAsync(),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
        };
    }

    private async Task<double> AverageMeasurementField(
        Expression<Func<Measurement, double>> fieldSelector,
        string fieldName,
        string? location,
        DateTime? start,
        DateTime? stop)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await FilteredMeasurements(context.Measurements.AsNoTracking(), location, start, stop)
            .TagWith($"Average {fieldName}")
            .AverageAsync(fieldSelector);
    }

    private async Task<double?> AverageMeasurementField(
        Expression<Func<Measurement, double?>> fieldSelector,
        string fieldName,
        string? location,
        DateTime? start,
        DateTime? stop)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await FilterNullValues(FilteredMeasurements(context.Measurements.AsNoTracking(), location, start, stop), fieldSelector)
            .TagWith($"Average {fieldName}")
            .AverageAsync(fieldSelector);
    }

    #endregion
}
