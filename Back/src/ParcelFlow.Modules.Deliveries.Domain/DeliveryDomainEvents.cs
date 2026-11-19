using ParcelFlow.BuildingBlocks.Domain;

namespace ParcelFlow.Modules.Deliveries.Domain;

public sealed record DeliveryAssignedDomainEvent(
    Guid DeliveryId,
    Guid ShipmentId,
    string TrackingNumber,
    string CourierId,
    DateTime OccurredOnUtc) : IDomainEvent;

/// <param name="IsFinal">true si era el último intento permitido y la entrega quedó en Failed.</param>
public sealed record DeliveryAttemptFailedDomainEvent(
    Guid DeliveryId,
    Guid ShipmentId,
    string TrackingNumber,
    int AttemptNumber,
    DeliveryFailureCode FailureCode,
    bool IsFinal,
    DateTime OccurredOnUtc) : IDomainEvent;

public sealed record ShipmentDeliveredDomainEvent(
    Guid DeliveryId,
    Guid ShipmentId,
    string TrackingNumber,
    string ReceivedBy,
    DateTime OccurredOnUtc) : IDomainEvent;

public sealed record DeliveryCancelledDomainEvent(
    Guid DeliveryId,
    Guid ShipmentId,
    string TrackingNumber,
    DateTime OccurredOnUtc) : IDomainEvent;
