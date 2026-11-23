using Microsoft.Extensions.Logging;
using ParcelFlow.BuildingBlocks.Application.IntegrationEvents;
using ParcelFlow.Modules.Deliveries.IntegrationEvents;
using ParcelFlow.Modules.Shipments.Application.Abstractions;
using ParcelFlow.Modules.Shipments.Domain;

namespace ParcelFlow.Modules.Shipments.Application.IntegrationEventHandlers;

/// <summary>
/// Deliveries confirmó la entrega → el envío pasa a Delivered.
///
/// Es el mismo patrón que un command handler (cargar → método del dominio → guardar),
/// pero lo dispara un evento de otro módulo en lugar de una petición HTTP.
///
/// Idempotente: Shipment.MarkAsDelivered ya ignora un segundo aviso.
/// Si el envío se canceló mientras tanto (carrera entre módulos), no se puede entregar:
/// se registra en el log y se descarta, en vez de reintentar para siempre algo que nunca funcionará.
/// </summary>
internal sealed class ShipmentDeliveredHandler(
    IShipmentRepository repository,
    IShipmentsUnitOfWork unitOfWork,
    ILogger<ShipmentDeliveredHandler> logger) : IIntegrationEventHandler<ShipmentDeliveredIntegrationEvent>
{
    public async Task Handle(ShipmentDeliveredIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        var shipment = await repository.GetByIdAsync(integrationEvent.ShipmentId, cancellationToken);

        if (shipment is null)
        {
            logger.LogWarning("Entrega confirmada para un envío inexistente {ShipmentId}", integrationEvent.ShipmentId);
            return;
        }

        if (shipment.Status == ShipmentStatus.Cancelled)
        {
            logger.LogWarning("El envío {TrackingNumber} se entregó pero estaba cancelado; se ignora",
                integrationEvent.TrackingNumber);
            return;
        }

        shipment.MarkAsDelivered(integrationEvent.OccurredOnUtc);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
