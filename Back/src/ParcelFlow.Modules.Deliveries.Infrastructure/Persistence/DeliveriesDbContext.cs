using Microsoft.EntityFrameworkCore;
using ParcelFlow.BuildingBlocks.Infrastructure.Outbox;
using ParcelFlow.Modules.Deliveries.Application.Abstractions;
using ParcelFlow.Modules.Deliveries.Domain;

namespace ParcelFlow.Modules.Deliveries.Infrastructure.Persistence;

/// <summary>DbContext del módulo Deliveries, schema "deliveries".</summary>
public sealed class DeliveriesDbContext(DbContextOptions<DeliveriesDbContext> options)
    : DbContext(options), IDeliveriesUnitOfWork
{
    public const string Schema = "deliveries";

    public DbSet<Delivery> Deliveries => Set<Delivery>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DeliveriesDbContext).Assembly);
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
    }

    Task IDeliveriesUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken) =>
        SaveChangesAsync(cancellationToken);
}
