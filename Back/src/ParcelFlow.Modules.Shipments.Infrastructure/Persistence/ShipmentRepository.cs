using Microsoft.EntityFrameworkCore;
using ParcelFlow.Modules.Shipments.Application.Abstractions;
using ParcelFlow.Modules.Shipments.Domain;

namespace ParcelFlow.Modules.Shipments.Infrastructure.Persistence;

/// <summary>
/// Implementación real de IShipmentRepository con EF Core. Es delgada a propósito:
/// solo traduce "dame / añade" a consultas de EF. No guarda (eso es el unit of work).
///
/// No hace falta un método Update: EF Core "rastrea" (change tracking) cada entidad que lee.
/// Si después llamas a shipment.Cancel(...), al hacer SaveChanges EF detecta qué columnas
/// cambiaron y genera el UPDATE solo.
/// </summary>
internal sealed class ShipmentRepository(ShipmentsDbContext dbContext) : IShipmentRepository
{
    public Task<Shipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Shipments.SingleOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<Shipment?> GetByTrackingNumberAsync(TrackingNumber trackingNumber, CancellationToken cancellationToken) =>
        // EF aplica el value converter al parámetro: se traduce a WHERE "TrackingNumber" = 'PF...'
        dbContext.Shipments.SingleOrDefaultAsync(s => s.TrackingNumber == trackingNumber, cancellationToken);

    public void Add(Shipment shipment) => dbContext.Shipments.Add(shipment);
}
