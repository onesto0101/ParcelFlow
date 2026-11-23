namespace ParcelFlow.BuildingBlocks.Application.IntegrationEvents;

/// <summary>
/// Reacciona a un integration event de OTRO módulo. Puede haber varios handlers por evento
/// (ej. ShipmentCancelled lo escuchan Tracking y Deliveries).
///
/// ⚠️ Debe ser IDEMPOTENTE: el outbox garantiza "al menos una vez", así que el mismo evento
/// puede llegar dos veces (ej. si falló otro handler y se reintenta). Procesarlo de nuevo
/// no debe duplicar nada.
/// </summary>
public interface IIntegrationEventHandler<in TEvent>
    where TEvent : IIntegrationEvent
{
    Task Handle(TEvent integrationEvent, CancellationToken cancellationToken);
}
