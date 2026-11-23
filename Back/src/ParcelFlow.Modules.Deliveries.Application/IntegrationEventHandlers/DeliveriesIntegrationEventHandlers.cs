using Microsoft.Extensions.Logging;
using ParcelFlow.BuildingBlocks.Application.IntegrationEvents;
using ParcelFlow.Modules.Deliveries.Application.Abstractions;
using ParcelFlow.Modules.Deliveries.Domain;
using ParcelFlow.Modules.Shipments.IntegrationEvents;
using ParcelFlow.Modules.Tracking.IntegrationEvents;

namespace ParcelFlow.Modules.Deliveries.Application.IntegrationEventHandlers;

/// <summary>
/// Cómo reacciona Deliveries a otros módulos:
///
///   ShipmentReadyForDelivery (Tracking)  → crea una entrega Pending (regla del diseño:
///                                          "solo se asigna cuando está listo para reparto")
///   ShipmentCancelled        (Shipments) → cancela la entrega, si existe
/// </summary>
internal sealed class DeliveriesIntegrationEventHandlers(
    IDeliveryRepository repository,
    IDeliveriesUnitOfWork unitOfWork,
    ILogger<DeliveriesIntegrationEventHandlers> logger) :
    IIntegrationEventHandler<ShipmentReadyForDeliveryIntegrationEvent>,
    IIntegrationEventHandler<ShipmentCancelledIntegrationEvent>
{
    public async Task Handle(ShipmentReadyForDeliveryIntegrationEvent e, CancellationToken cancellationToken)
    {
        // Idempotente Y de negocio a la vez: un paquete puede pasar por varios centros de
        // distribución (InTransit ↔ AtDistributionCenter), pero solo tiene UNA entrega.
        if (await repository.ExistsForShipmentAsync(e.ShipmentId, cancellationToken))
            return;

        repository.Add(Delivery.CreatePending(e.ShipmentId, e.TrackingNumber, e.OccurredOnUtc));
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(ShipmentCancelledIntegrationEvent e, CancellationToken cancellationToken)
    {
        var delivery = await repository.GetByShipmentIdAsync(e.ShipmentId, cancellationToken);

        // Lo normal: se cancela antes de llegar al centro y todavía no hay entrega.
        if (delivery is null) return;

        if (delivery.Status == DeliveryStatus.Delivered)
        {
            // Carrera entre módulos: se entregó y casi a la vez se canceló el envío.
            // La entrega física ya ocurrió; no se puede "des-entregar". Se registra y se sigue.
            logger.LogWarning("El envío {TrackingNumber} se canceló pero ya estaba entregado", e.TrackingNumber);
            return;
        }

        delivery.Cancel(e.OccurredOnUtc); // idempotente en el dominio si ya estaba cancelada
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
