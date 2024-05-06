using Microsoft.EntityFrameworkCore;
using SensorServer.Models;

namespace SensorServer;

public sealed class ApplicationDbContext(IConfiguration configuration, ILoggerFactory loggerFactory) : DbContext
{
    private static bool _didLogConnectionString;

    public DbSet<Measurement> Measurements { get; init; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!_didLogConnectionString)
        {
            loggerFactory.CreateLogger<ApplicationDbContext>()
                .LogInformation("Using database connection string: {ConnectionString}", connectionString);
            _didLogConnectionString = true;
        }

        optionsBuilder.UseSqlite(connectionString).UseLoggerFactory(loggerFactory);
    }
}
