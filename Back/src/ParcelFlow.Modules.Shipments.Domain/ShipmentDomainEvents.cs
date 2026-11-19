using ParcelFlow.BuildingBlocks.Domain;

namespace ParcelFlow.Modules.Shipments.Domain;

public sealed record ShipmentCreatedDomainEvent(
    Guid ShipmentId,
    string TrackingNumber,
    string OriginCity,
    string DestinationCity,
    DateTime OccurredOnUtc) : IDomainEvent;

public sealed record ShipmentCancelledDomainEvent(
    Guid ShipmentId,
    string TrackingNumber,
    string Reason,
    DateTime OccurredOnUtc) : IDomainEvent;
