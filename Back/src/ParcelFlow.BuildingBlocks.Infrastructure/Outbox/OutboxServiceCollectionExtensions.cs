using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ParcelFlow.BuildingBlocks.Infrastructure.Outbox;

public static class OutboxServiceCollectionExtensions
{
    /// <summary>
    /// Registra el outbox de un módulo: su timbre y su procesador en segundo plano.
    /// El bus es compartido (TryAdd: aunque lo pidan los tres módulos, se registra una sola vez).
    /// </summary>
    public static IServiceCollection AddOutbox<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<IEventBus, InMemoryEventBus>();
        services.AddSingleton<OutboxSignal<TDbContext>>();
        services.AddHostedService<OutboxProcessor<TDbContext>>();
        return services;
    }

    /// <summary>
    /// Engancha el <see cref="OutboxInterceptor"/> al DbContext del módulo, con el mapper del módulo.
    /// Se llama dentro de AddDbContext((serviceProvider, options) => ...).
    /// </summary>
    public static DbContextOptionsBuilder AddOutboxInterceptor<TDbContext>(
        this DbContextOptionsBuilder options, IServiceProvider serviceProvider, IDomainEventMapper mapper)
        where TDbContext : DbContext =>
        options.AddInterceptors(new OutboxInterceptor(
            mapper, serviceProvider.GetRequiredService<OutboxSignal<TDbContext>>()));
}
