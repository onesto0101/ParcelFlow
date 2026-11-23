using Microsoft.EntityFrameworkCore;
using ParcelFlow.Modules.Deliveries.Application.Abstractions;
using ParcelFlow.Modules.Deliveries.Domain;

namespace ParcelFlow.Modules.Deliveries.Infrastructure.Persistence;

/// <summary>Carga siempre la entrega con sus intentos: el aggregate los necesita para contar (máximo 3).</summary>
internal sealed class DeliveryRepository(DeliveriesDbContext dbContext) : IDeliveryRepository
{
    public Task<Delivery?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        WithAttempts().SingleOrDefaultAsync(d => d.Id == id, cancellationToken);

    public Task<Delivery?> GetByShipmentIdAsync(Guid shipmentId, CancellationToken cancellationToken) =>
        WithAttempts().SingleOrDefaultAsync(d => d.ShipmentId == shipmentId, cancellationToken);

    public Task<bool> ExistsForShipmentAsync(Guid shipmentId, CancellationToken cancellationToken) =>
        dbContext.Deliveries.AnyAsync(d => d.ShipmentId == shipmentId, cancellationToken);

    public void Add(Delivery delivery) => dbContext.Deliveries.Add(delivery);

    private IQueryable<Delivery> WithAttempts() =>
        dbContext.Deliveries.Include(d => d.Attempts.OrderBy(a => a.AttemptNumber));
}
