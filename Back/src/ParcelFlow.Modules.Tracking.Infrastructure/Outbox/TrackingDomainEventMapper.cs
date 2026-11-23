using ParcelFlow.BuildingBlocks.Application.IntegrationEvents;
using ParcelFlow.BuildingBlocks.Domain;
using ParcelFlow.BuildingBlocks.Infrastructure.Outbox;
using ParcelFlow.Modules.Tracking.Domain;
using ParcelFlow.Modules.Tracking.IntegrationEvents;

namespace ParcelFlow.Modules.Tracking.Infrastructure.Outbox;

/// <summary>
/// De los dos domain events de Tracking, solo uno sale del módulo:
/// - ShipmentArrivedAtCenter → ShipmentReadyForDelivery (Deliveries lo necesita).
/// - ShipmentStatusChanged   → no se publica: hoy nadie fuera lo necesita. Publicar "por si acaso"
///                             crea contratos que luego no puedes cambiar sin romper a alguien.
/// </summary>
internal sealed class TrackingDomainEventMapper : IDomainEventMapper
{
    public IIntegrationEvent? Map(IDomainEvent domainEvent) => domainEvent switch
    {
        ShipmentArrivedAtCenterDomainEvent e => new ShipmentReadyForDeliveryIntegrationEvent(
            e.ShipmentId, e.TrackingNumber, e.Location, e.OccurredOnUtc),

        _ => null,
    };
}
