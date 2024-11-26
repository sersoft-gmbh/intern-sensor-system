using Microsoft.EntityFrameworkCore;
using SensorServer.Models;

namespace SensorServer;

public sealed class ApplicationDbContext(IConfiguration configuration, ILoggerFactory loggerFactory) : DbContext
{
    private static readonly Lock DidLogConnectionStringLock = new();
    private static bool _didLogConnectionString;

    public DbSet<Measurement> Measurements { get; init; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        bool shouldLog;
        lock (DidLogConnectionStringLock)
        {
            shouldLog = !_didLogConnectionString;
            if (!_didLogConnectionString) _didLogConnectionString = true;
        }
        if (shouldLog)
            loggerFactory.CreateLogger<ApplicationDbContext>()
                .LogInformation("Using database connection string: {ConnectionString}", connectionString);

        optionsBuilder.UseSqlite(connectionString).UseLoggerFactory(loggerFactory);
    }
}
