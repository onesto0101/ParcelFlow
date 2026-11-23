using Microsoft.EntityFrameworkCore;
using ParcelFlow.BuildingBlocks.Infrastructure.Outbox;
using ParcelFlow.Modules.Tracking.Application.Abstractions;
using ParcelFlow.Modules.Tracking.Domain;

namespace ParcelFlow.Modules.Tracking.Infrastructure.Persistence;

/// <summary>
/// DbContext del módulo Tracking, en su propio schema "tracking". Misma receta que ShipmentsDbContext:
/// es también el unit of work y tiene su propia tabla de outbox.
/// </summary>
public sealed class TrackingDbContext(DbContextOptions<TrackingDbContext> options)
    : DbContext(options), ITrackingUnitOfWork
{
    public const string Schema = "tracking";

    public DbSet<ShipmentTracking> ShipmentTrackings => Set<ShipmentTracking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TrackingDbContext).Assembly);
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
    }

    Task ITrackingUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken) =>
        SaveChangesAsync(cancellationToken);
}
