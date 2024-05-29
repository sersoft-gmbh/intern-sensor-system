using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SensorServer.Models;
using UnitsNet;

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

    public async Task<Measurement?> GetMeasurement(long id, CancellationToken cancellationToken = default)
        => await dbContext.Measurements.FindAsync(id, cancellationToken);

    public async Task<Measurement?> GetLatestMeasurement(string? location, CancellationToken cancellationToken = default)
    {
        return await FilteredMeasurements(dbContext.Measurements.AsNoTracking(), location, null, null)
            .TagWith(nameof(GetLatestMeasurement))
            .OrderByDescending(m => m.Date)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<MeasurementCounts> GetMeasurementCounts(DateTime? start = null, DateTime? stop = null, CancellationToken cancellationToken = default)
    {
        var measurements = FilteredMeasurements(dbContext.Measurements.AsNoTracking(), null, start, stop)
            .TagWith(nameof(GetMeasurementCounts));
        var total = await measurements
            .TagWith("Total")
            .LongCountAsync(cancellationToken);
        var perLocation = await measurements
            .TagWith("Per Location")
            .GroupBy(m => m.Location)
            .Select(x => new { Location = x.Key, Count = x.LongCount() })
            .ToDictionaryAsync(g => g.Location, g => g.Count, cancellationToken);
        return new MeasurementCounts
        {
            Total = total,
            PerLocation = perLocation,
        };
    }

    public async Task<MeasurementStatistics> GetMeasurementStatistics(
        string? location = null,
        DateTime? start = null,
        DateTime? stop = null,
        CancellationToken cancellationToken = default)
    {
        long count, countWithPressure;
        await using (var context = await dbContextFactory.CreateDbContextAsync(cancellationToken))
        {
            var filtered = FilteredMeasurements(context.Measurements.AsNoTracking(), location, start, stop)
                .TagWith(nameof(GetMeasurementStatistics));
            count = await filtered.TagWith("Total").LongCountAsync(cancellationToken);
            if (count == 0) return new MeasurementStatistics();
            countWithPressure = await filtered.Where(m => m.PressureHectopascals != null).TagWith("Total with Pressure").LongCountAsync(cancellationToken);
        }

        var averageTemperatureCelsius = AverageMeasurementField(m => m.TemperatureCelsius, "Temperature", location, start, stop, cancellationToken);
        var averageHumidityPercent = AverageMeasurementField(m => m.HumidityPercent, "Humidity", location, start, stop, cancellationToken);
        var averagePressureHectopascalsTask = AverageMeasurementField(m => m.PressureHectopascals, "Pressure", location, start, stop, cancellationToken);
        var minTemperature = StatisticsMeasurement(m => m.TemperatureCelsius, "Temperature", StatisticKind.Min, count, location, start, stop, cancellationToken: cancellationToken);
        var maxTemperature = StatisticsMeasurement(m => m.TemperatureCelsius, "Temperature", StatisticKind.Max, count, location, start, stop, cancellationToken: cancellationToken);
        var medianTemperature = StatisticsMeasurement(m => m.TemperatureCelsius, "Temperature", StatisticKind.Median, count, location, start, stop, cancellationToken: cancellationToken);
        var minHumidity = StatisticsMeasurement(m => m.HumidityPercent, "Humidity", StatisticKind.Min, count, location, start, stop, cancellationToken: cancellationToken);
        var maxHumidity = StatisticsMeasurement(m => m.HumidityPercent, "Humidity", StatisticKind.Max, count, location, start, stop, cancellationToken: cancellationToken);
        var medianHumidity = StatisticsMeasurement(m => m.HumidityPercent, "Humidity", StatisticKind.Median, count, location, start, stop, cancellationToken: cancellationToken);
        var minPressure = StatisticsMeasurement(m => m.PressureHectopascals, "Pressure", StatisticKind.Min, countWithPressure, location, start, stop, true, cancellationToken);
        var maxPressure = StatisticsMeasurement(m => m.PressureHectopascals, "Pressure", StatisticKind.Max, countWithPressure, location, start, stop, true, cancellationToken);
        var medianPressure = StatisticsMeasurement(m => m.PressureHectopascals, "Pressure", StatisticKind.Median, countWithPressure, location, start, stop, true, cancellationToken);

        var averagePressureHectopascals = await averagePressureHectopascalsTask;

        return new MeasurementStatistics(Temperature.FromDegreesCelsius(await averageTemperatureCelsius),
            RelativeHumidity.FromPercent(await averageHumidityPercent * 100),
            averagePressureHectopascals.HasValue ? Pressure.FromHectopascals(averagePressureHectopascals.Value) : null)
        {
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

    public async Task<Measurement> Add(Measurement measurement, CancellationToken cancellationToken = default)
    {
        var result = await dbContext.Measurements.AddAsync(measurement, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return result.Entity;
    }

    #region Helpers

    private static IQueryable<Measurement> FilteredMeasurements(IQueryable<Measurement> measurements, string? location, DateTime? start, DateTime? stop)
    {
        var filtered = measurements;
        if (location != null)
            filtered = filtered.Where(m => m.Location == location);

        if (start != null)
            filtered = filtered.Where(m => m.Date >= start);

        if (stop != null)
            filtered = filtered.Where(m => m.Date <= stop);

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
        bool filterForNull = false,
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var queryable = FilteredMeasurements(context.Measurements.AsNoTracking(), location, start, stop)
            .TagWith($"{kind} {fieldName}");
        if (filterForNull) queryable = FilterNullValues(queryable, fieldSelector);
        return kind switch
        {
            StatisticKind.Min => await queryable.OrderBy(fieldSelector).ThenBy(m => m.Id).FirstOrDefaultAsync(cancellationToken),
            StatisticKind.Max => await queryable.OrderByDescending(fieldSelector).ThenByDescending(m => m.Id).FirstOrDefaultAsync(cancellationToken),
            StatisticKind.Median => await queryable.OrderBy(fieldSelector).Skip((int)(count / 2)).FirstOrDefaultAsync(cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
        };
    }

    private async Task<double> AverageMeasurementField(
        Expression<Func<Measurement, double>> fieldSelector,
        string fieldName,
        string? location,
        DateTime? start,
        DateTime? stop,
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await FilteredMeasurements(context.Measurements.AsNoTracking(), location, start, stop)
            .TagWith($"Average {fieldName}")
            .AverageAsync(fieldSelector, cancellationToken);
    }

    private async Task<double?> AverageMeasurementField(
        Expression<Func<Measurement, double?>> fieldSelector,
        string fieldName,
        string? location,
        DateTime? start,
        DateTime? stop,
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await FilterNullValues(FilteredMeasurements(context.Measurements.AsNoTracking(), location, start, stop), fieldSelector)
            .TagWith($"Average {fieldName}")
            .AverageAsync(fieldSelector, cancellationToken);
    }

    #endregion
}
