using ParcelFlow.BuildingBlocks.Application.IntegrationEvents;

namespace ParcelFlow.Modules.Tracking.IntegrationEvents;

/// <summary>
/// El paquete llegó a un centro de distribución: está listo para la última milla.
/// Deliveries crea una entrega pendiente de asignar repartidor.
///
/// Fíjate en el nombre: dentro de Tracking el hecho es "llegó al centro"
/// (ShipmentArrivedAtCenterDomainEvent). Hacia fuera se publica con el significado que
/// le importa a los demás: "listo para reparto" (así lo llama el documento de diseño, §6.2).
/// </summary>
public sealed record ShipmentReadyForDeliveryIntegrationEvent(
    Guid ShipmentId,
    string TrackingNumber,
    string Location,
    DateTime OccurredOnUtc) : IIntegrationEvent;
