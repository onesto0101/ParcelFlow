using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ParcelFlow.Modules.Deliveries.Infrastructure.Persistence;

/// <summary>Solo para "dotnet ef" (migraciones). Igual que ShipmentsDbContextFactory.</summary>
internal sealed class DeliveriesDbContextFactory : IDesignTimeDbContextFactory<DeliveriesDbContext>
{
    private const string LocalConnectionString =
        "Host=localhost;Port=5432;Database=parcelflow;Username=parcelflow;Password=parcelflow";

    public DeliveriesDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("PARCELFLOW_DB") ?? LocalConnectionString;

        var options = new DbContextOptionsBuilder<DeliveriesDbContext>();
        DeliveriesInfrastructureModule.ConfigureDbContext(options, connectionString);

        return new DeliveriesDbContext(options.Options);
    }
}
