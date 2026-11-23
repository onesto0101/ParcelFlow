using ParcelFlow.Modules.Deliveries.Application.Abstractions;
using ParcelFlow.Modules.Deliveries.Domain;

namespace ParcelFlow.Modules.Deliveries.UnitTests.Application;

internal sealed class FakeDeliveryRepository : IDeliveryRepository
{
    public List<Delivery> Items { get; } = [];

    public Task<Delivery?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Items.SingleOrDefault(d => d.Id == id));

    public Task<Delivery?> GetByShipmentIdAsync(Guid shipmentId, CancellationToken cancellationToken) =>
        Task.FromResult(Items.SingleOrDefault(d => d.ShipmentId == shipmentId));

    public Task<bool> ExistsForShipmentAsync(Guid shipmentId, CancellationToken cancellationToken) =>
        Task.FromResult(Items.Any(d => d.ShipmentId == shipmentId));

    public void Add(Delivery delivery) => Items.Add(delivery);
}

internal sealed class FakeDeliveriesUnitOfWork : IDeliveriesUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}
