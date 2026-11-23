using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ParcelFlow.Modules.Tracking.Infrastructure.Persistence;

/// <summary>Solo para "dotnet ef" (migraciones). Igual que ShipmentsDbContextFactory.</summary>
internal sealed class TrackingDbContextFactory : IDesignTimeDbContextFactory<TrackingDbContext>
{
    private const string LocalConnectionString =
        "Host=localhost;Port=5432;Database=parcelflow;Username=parcelflow;Password=parcelflow";

    public TrackingDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("PARCELFLOW_DB") ?? LocalConnectionString;

        var options = new DbContextOptionsBuilder<TrackingDbContext>();
        TrackingInfrastructureModule.ConfigureDbContext(options, connectionString);

        return new TrackingDbContext(options.Options);
    }
}
