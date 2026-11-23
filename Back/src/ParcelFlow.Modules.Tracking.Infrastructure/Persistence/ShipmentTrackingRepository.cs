using Microsoft.EntityFrameworkCore;
using ParcelFlow.Modules.Tracking.Application.Abstractions;
using ParcelFlow.Modules.Tracking.Domain;

namespace ParcelFlow.Modules.Tracking.Infrastructure.Persistence;

/// <summary>
/// Siempre carga el aggregate COMPLETO (Include de los movimientos): un aggregate a medias
/// no podría proteger sus reglas.
/// </summary>
internal sealed class ShipmentTrackingRepository(TrackingDbContext dbContext) : IShipmentTrackingRepository
{
    public Task<ShipmentTracking?> GetByShipmentIdAsync(Guid shipmentId, CancellationToken cancellationToken) =>
        WithEvents().SingleOrDefaultAsync(t => t.ShipmentId == shipmentId, cancellationToken);

    public Task<ShipmentTracking?> GetByTrackingNumberAsync(string trackingNumber, CancellationToken cancellationToken) =>
        WithEvents().SingleOrDefaultAsync(t => t.TrackingNumber == trackingNumber, cancellationToken);

    public Task<bool> ExistsForShipmentAsync(Guid shipmentId, CancellationToken cancellationToken) =>
        dbContext.ShipmentTrackings.AnyAsync(t => t.ShipmentId == shipmentId, cancellationToken);

    public void Add(ShipmentTracking tracking) => dbContext.ShipmentTrackings.Add(tracking);

    // "Filtered include" con orden: SQL no garantiza ningún orden si no se pide, y el
    // historial del aggregate tiene que estar en orden cronológico.
    private IQueryable<ShipmentTracking> WithEvents() =>
        dbContext.ShipmentTrackings.Include(t => t.Events.OrderBy(e => e.OccurredAt).ThenBy(e => e.Id));
}
