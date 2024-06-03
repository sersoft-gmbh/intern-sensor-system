using SensorServer.Helpers;

namespace SensorServer.Services;

public sealed class DbCheckpointService(
    IServiceScopeFactory serviceScopeFactory,
    IConfiguration configuration,
    ILogger<DbCheckpointService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var configuredInterval = configuration.GetValue<string>("DbWalCheckpointInterval");
        var timeInterval = configuredInterval is not null ? TimeSpan.Parse(configuredInterval) : TimeSpan.FromHours(1);
        using var timer = new PeriodicTimer(timeInterval);
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await using var asyncScope = serviceScopeFactory.CreateAsyncScope();
                await using var dbContext = asyncScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await dbContext.Database.CheckpointDb(cancellationToken: stoppingToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error during database checkpointing");
            }
        }
    }
}
