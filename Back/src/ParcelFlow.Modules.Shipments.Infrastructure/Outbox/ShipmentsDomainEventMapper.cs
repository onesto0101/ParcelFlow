using ParcelFlow.BuildingBlocks.Application.IntegrationEvents;
using ParcelFlow.BuildingBlocks.Domain;
using ParcelFlow.BuildingBlocks.Infrastructure.Outbox;
using ParcelFlow.Modules.Shipments.Domain;
using ParcelFlow.Modules.Shipments.IntegrationEvents;

namespace ParcelFlow.Modules.Shipments.Infrastructure.Outbox;

/// <summary>
/// Domain events de Shipments → integration events públicos.
/// Aquí está la "traducción": hoy los campos coinciden, pero si mañana el domain event cambia
/// (ej. pasa a llevar un Address completo), el contrato público sigue igual y nadie se rompe.
/// </summary>
internal sealed class ShipmentsDomainEventMapper : IDomainEventMapper
{
    public IIntegrationEvent? Map(IDomainEvent domainEvent) => domainEvent switch
    {
        ShipmentCreatedDomainEvent e => new ShipmentCreatedIntegrationEvent(
            e.ShipmentId, e.TrackingNumber, e.OriginCity, e.DestinationCity, e.OccurredOnUtc),

        ShipmentCancelledDomainEvent e => new ShipmentCancelledIntegrationEvent(
            e.ShipmentId, e.TrackingNumber, e.Reason, e.OccurredOnUtc),

        _ => null,
    };
}
