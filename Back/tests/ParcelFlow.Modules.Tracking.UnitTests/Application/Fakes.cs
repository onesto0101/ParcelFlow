using ParcelFlow.Modules.Tracking.Application.Abstractions;
using ParcelFlow.Modules.Tracking.Domain;

namespace ParcelFlow.Modules.Tracking.UnitTests.Application;

/// <summary>Repositorio en memoria (misma idea que FakeShipmentRepository).</summary>
internal sealed class FakeTrackingRepository : IShipmentTrackingRepository
{
    public List<ShipmentTracking> Items { get; } = [];

    public Task<ShipmentTracking?> GetByShipmentIdAsync(Guid shipmentId, CancellationToken cancellationToken) =>
        Task.FromResult(Items.SingleOrDefault(t => t.ShipmentId == shipmentId));

    public Task<ShipmentTracking?> GetByTrackingNumberAsync(string trackingNumber, CancellationToken cancellationToken) =>
        Task.FromResult(Items.SingleOrDefault(t => t.TrackingNumber == trackingNumber));

    public Task<bool> ExistsForShipmentAsync(Guid shipmentId, CancellationToken cancellationToken) =>
        Task.FromResult(Items.Any(t => t.ShipmentId == shipmentId));

    public void Add(ShipmentTracking tracking) => Items.Add(tracking);
}

internal sealed class FakeTrackingUnitOfWork : ITrackingUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}

internal static class TrackingData
{
    public static readonly DateTime Now = new(2026, 11, 23, 10, 0, 0, DateTimeKind.Utc);
    public const string TrackingNumber = "PF23456789AB";

    public static ShipmentTracking Started(Guid? shipmentId = null) =>
        ShipmentTracking.Start(shipmentId ?? Guid.NewGuid(), TrackingNumber, new Location("Monterrey"), Now);
}
