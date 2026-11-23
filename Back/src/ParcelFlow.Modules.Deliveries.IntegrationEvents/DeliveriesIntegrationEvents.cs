using ParcelFlow.BuildingBlocks.Application.IntegrationEvents;

namespace ParcelFlow.Modules.Deliveries.IntegrationEvents;

/// <summary>Se asignó un repartidor: el paquete sale a reparto. Tracking → OutForDelivery.</summary>
public sealed record DeliveryAssignedIntegrationEvent(
    Guid DeliveryId,
    Guid ShipmentId,
    string TrackingNumber,
    string CourierId,
    DateTime OccurredOnUtc) : IIntegrationEvent;

/// <summary>
/// Un intento de entrega falló. Tracking → DeliveryFailed.
/// <paramref name="FailureCode"/> va como texto ("RecipientAbsent"), no como el enum del dominio:
/// así Tracking no necesita conocer los tipos de Deliveries.
/// </summary>
public sealed record DeliveryAttemptFailedIntegrationEvent(
    Guid DeliveryId,
    Guid ShipmentId,
    string TrackingNumber,
    int AttemptNumber,
    string FailureCode,
    bool IsFinal,
    DateTime OccurredOnUtc) : IIntegrationEvent;

/// <summary>Entrega confirmada. Tracking → Delivered y Shipments → Delivered.</summary>
public sealed record ShipmentDeliveredIntegrationEvent(
    Guid DeliveryId,
    Guid ShipmentId,
    string TrackingNumber,
    string ReceivedBy,
    DateTime OccurredOnUtc) : IIntegrationEvent;
