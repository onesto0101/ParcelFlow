using ParcelFlow.BuildingBlocks.Application.IntegrationEvents;
using ParcelFlow.BuildingBlocks.Domain;
using ParcelFlow.BuildingBlocks.Infrastructure.Outbox;
using ParcelFlow.Modules.Deliveries.Domain;
using ParcelFlow.Modules.Deliveries.IntegrationEvents;

namespace ParcelFlow.Modules.Deliveries.Infrastructure.Outbox;

/// <summary>
/// DeliveryCancelled no se publica: la cancelación la originó Shipments, y los demás ya se
/// enteraron por ShipmentCancelled. Publicarla otra vez solo crearía eco entre módulos.
/// </summary>
internal sealed class DeliveriesDomainEventMapper : IDomainEventMapper
{
    public IIntegrationEvent? Map(IDomainEvent domainEvent) => domainEvent switch
    {
        DeliveryAssignedDomainEvent e => new DeliveryAssignedIntegrationEvent(
            e.DeliveryId, e.ShipmentId, e.TrackingNumber, e.CourierId, e.OccurredOnUtc),

        DeliveryAttemptFailedDomainEvent e => new DeliveryAttemptFailedIntegrationEvent(
            e.DeliveryId, e.ShipmentId, e.TrackingNumber, e.AttemptNumber, e.FailureCode.ToString(), e.IsFinal, e.OccurredOnUtc),

        ShipmentDeliveredDomainEvent e => new ShipmentDeliveredIntegrationEvent(
            e.DeliveryId, e.ShipmentId, e.TrackingNumber, e.ReceivedBy, e.OccurredOnUtc),

        _ => null,
    };
}
