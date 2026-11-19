using ParcelFlow.BuildingBlocks.Domain;

namespace ParcelFlow.Modules.Tracking.Domain;

/// <summary>Se lanza en CADA cambio de estado.</summary>
public sealed record ShipmentStatusChangedDomainEvent(
    Guid ShipmentTrackingId,
    Guid ShipmentId,
    string TrackingNumber,
    TrackingStatus PreviousStatus,
    TrackingStatus NewStatus,
    string Location,
    DateTime OccurredOnUtc) : IDomainEvent;

/// <summary>
/// El paquete llegó a un centro de distribución. Según el diseño, a partir de aquí
/// el paquete está "listo para reparto" y el módulo Deliveries debe enterarse.
/// </summary>
public sealed record ShipmentArrivedAtCenterDomainEvent(
    Guid ShipmentTrackingId,
    Guid ShipmentId,
    string TrackingNumber,
    string Location,
    DateTime OccurredOnUtc) : IDomainEvent;
