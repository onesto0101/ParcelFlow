using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ParcelFlow.BuildingBlocks.Application.IntegrationEvents;

namespace ParcelFlow.BuildingBlocks.Infrastructure.Outbox;

/// <summary>
/// Bus en memoria: busca en DI todos los IIntegrationEventHandler&lt;TEvento&gt; y los ejecuta
/// uno detrás de otro.
///
/// Crea un scope propio por evento: los handlers piden los DbContexts (Scoped) de SUS módulos,
/// y no deben compartir instancia con el DbContext del outbox que está publicando.
///
/// Si un handler lanza, la excepción sube al procesador y el mensaje se reintenta entero,
/// incluidos los handlers que ya funcionaron. Por eso tienen que ser idempotentes.
/// </summary>
internal sealed class InMemoryEventBus(IServiceScopeFactory scopeFactory) : IEventBus
{
    public async Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        // Mismo truco que el Mediator: el tipo concreto del evento solo se conoce en ejecución.
        var handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(integrationEvent.GetType());
        var handle = handlerType.GetMethod("Handle")!;

        await using var scope = scopeFactory.CreateAsyncScope();

        foreach (var handler in scope.ServiceProvider.GetServices(handlerType))
        {
            await (Task)handle.Invoke(
                handler,
                BindingFlags.DoNotWrapExceptions,
                binder: null,
                parameters: [integrationEvent, cancellationToken],
                culture: null)!;
        }
    }
}
