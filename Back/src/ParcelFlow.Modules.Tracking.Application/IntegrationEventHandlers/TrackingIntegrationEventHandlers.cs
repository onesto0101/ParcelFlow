using Microsoft.Extensions.Logging;
using ParcelFlow.BuildingBlocks.Application.IntegrationEvents;
using ParcelFlow.Modules.Deliveries.IntegrationEvents;
using ParcelFlow.Modules.Shipments.IntegrationEvents;
using ParcelFlow.Modules.Tracking.Application.Abstractions;
using ParcelFlow.Modules.Tracking.Domain;

namespace ParcelFlow.Modules.Tracking.Application.IntegrationEventHandlers;

/// <summary>
/// Cómo reacciona Tracking a lo que pasa en otros módulos. Una sola clase que implementa
/// cinco IIntegrationEventHandler&lt;T&gt;: comparten dependencias y la lógica de "aplicar un estado".
///
///   ShipmentCreated        (Shipments)  → empieza el seguimiento (estado Created)
///   ShipmentCancelled      (Shipments)  → Cancelled
///   DeliveryAssigned       (Deliveries) → OutForDelivery
///   DeliveryAttemptFailed  (Deliveries) → DeliveryFailed
///   ShipmentDelivered      (Deliveries) → Delivered
///
/// IDEMPOTENCIA: todos comprueban antes de actuar. Si el evento llega dos veces, la segunda vez
/// el seguimiento ya existe / la transición ya no es válida → se ignora sin error.
/// </summary>
internal sealed class TrackingIntegrationEventHandlers(
    IShipmentTrackingRepository repository,
    ITrackingUnitOfWork unitOfWork,
    ILogger<TrackingIntegrationEventHandlers> logger) :
    IIntegrationEventHandler<ShipmentCreatedIntegrationEvent>,
    IIntegrationEventHandler<ShipmentCancelledIntegrationEvent>,
    IIntegrationEventHandler<DeliveryAssignedIntegrationEvent>,
    IIntegrationEventHandler<DeliveryAttemptFailedIntegrationEvent>,
    IIntegrationEventHandler<ShipmentDeliveredIntegrationEvent>
{
    public async Task Handle(ShipmentCreatedIntegrationEvent e, CancellationToken cancellationToken)
    {
        if (await repository.ExistsForShipmentAsync(e.ShipmentId, cancellationToken))
            return; // ya procesado (evento repetido)

        var tracking = ShipmentTracking.Start(e.ShipmentId, e.TrackingNumber, new Location(e.OriginCity), e.OccurredOnUtc);

        repository.Add(tracking);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public Task Handle(ShipmentCancelledIntegrationEvent e, CancellationToken cancellationToken) =>
        ApplyAsync(e.ShipmentId, TrackingStatus.Cancelled, $"Envío cancelado. Motivo: {e.Reason}", e.OccurredOnUtc, cancellationToken);

    public Task Handle(DeliveryAssignedIntegrationEvent e, CancellationToken cancellationToken) =>
        ApplyAsync(e.ShipmentId, TrackingStatus.OutForDelivery, $"En reparto con el repartidor {e.CourierId}.", e.OccurredOnUtc, cancellationToken);

    public Task Handle(DeliveryAttemptFailedIntegrationEvent e, CancellationToken cancellationToken)
    {
        var description = $"Intento de entrega {e.AttemptNumber} fallido: {DescribeFailure(e.FailureCode)}."
            + (e.IsFinal ? " Se agotaron los intentos de entrega." : " Se reprogramará.");

        return ApplyAsync(e.ShipmentId, TrackingStatus.DeliveryFailed, description, e.OccurredOnUtc, cancellationToken);
    }

    public Task Handle(ShipmentDeliveredIntegrationEvent e, CancellationToken cancellationToken) =>
        ApplyAsync(e.ShipmentId, TrackingStatus.Delivered, $"Entregado. Recibió: {e.ReceivedBy}.", e.OccurredOnUtc, cancellationToken);

    /// <summary>
    /// Aplica un estado que viene de otro módulo, en la ubicación actual del paquete.
    /// </summary>
    private async Task ApplyAsync(
        Guid shipmentId, TrackingStatus status, string description, DateTime occurredAt, CancellationToken cancellationToken)
    {
        var tracking = await repository.GetByShipmentIdAsync(shipmentId, cancellationToken);

        if (tracking is null)
        {
            // Ej. envíos creados antes de que existiera el módulo Tracking.
            logger.LogWarning("Sin seguimiento para el envío {ShipmentId}; se ignora {Status}", shipmentId, status);
            return;
        }

        if (!tracking.CanTransitionTo(status))
        {
            // Evento repetido (ya está en ese estado) o que llega en un estado que no lo admite
            // (ej. asignan repartidor mientras el paquete aún va en tránsito). Reintentar no lo
            // arreglaría, así que se registra y se descarta.
            logger.LogInformation("Seguimiento {TrackingNumber}: se ignora {From} → {To}",
                tracking.TrackingNumber, tracking.CurrentStatus, status);
            return;
        }

        // El dominio exige orden cronológico. Si el evento trae una hora anterior al último
        // movimiento (relojes, retrasos del outbox...), se registra con la hora del último.
        var at = occurredAt < tracking.LastUpdatedAt ? tracking.LastUpdatedAt : occurredAt;

        tracking.RegisterEvent(status, tracking.CurrentLocation, new TrackingDescription(Truncate(description)), at);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string DescribeFailure(string failureCode) => failureCode switch
    {
        "RecipientAbsent" => "destinatario ausente",
        "WrongAddress" => "dirección incorrecta",
        "Refused" => "el destinatario lo rechazó",
        "Inaccessible" => "domicilio inaccesible",
        _ => "otro motivo",
    };

    private static string Truncate(string text) =>
        text.Length <= TrackingDescription.MaxLength ? text : text[..(TrackingDescription.MaxLength - 1)] + "…";
}
