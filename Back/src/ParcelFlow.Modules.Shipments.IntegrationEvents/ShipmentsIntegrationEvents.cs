using ParcelFlow.BuildingBlocks.Application.IntegrationEvents;

namespace ParcelFlow.Modules.Shipments.IntegrationEvents;

/// <summary>Se registró un envío. Tracking empieza su seguimiento.</summary>
public sealed record ShipmentCreatedIntegrationEvent(
    Guid ShipmentId,
    string TrackingNumber,
    string OriginCity,
    string DestinationCity,
    DateTime OccurredOnUtc) : IIntegrationEvent;

/// <summary>Se canceló un envío. Tracking lo marca como cancelado y Deliveries cancela la entrega.</summary>
public sealed record ShipmentCancelledIntegrationEvent(
    Guid ShipmentId,
    string TrackingNumber,
    string Reason,
    DateTime OccurredOnUtc) : IIntegrationEvent;
