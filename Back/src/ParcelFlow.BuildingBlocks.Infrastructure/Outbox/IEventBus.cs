using ParcelFlow.BuildingBlocks.Application.IntegrationEvents;

namespace ParcelFlow.BuildingBlocks.Infrastructure.Outbox;

/// <summary>
/// Entrega un integration event a quien lo escuche. Hoy es en memoria (mismo proceso). Mañana
/// podría ser RabbitMQ o Azure Service Bus sin tocar los módulos: solo cambiaría esta implementación.
/// </summary>
public interface IEventBus
{
    Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken);
}
