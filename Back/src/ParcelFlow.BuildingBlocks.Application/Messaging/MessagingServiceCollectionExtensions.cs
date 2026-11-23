using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ParcelFlow.BuildingBlocks.Application.IntegrationEvents;

namespace ParcelFlow.BuildingBlocks.Application.Messaging;

/// <summary>
/// Métodos para registrar el mediator y los handlers en el contenedor de dependencias (DI).
/// Son métodos de extensión: se usan como services.AddMessaging() en Program.cs.
/// </summary>
public static class MessagingServiceCollectionExtensions
{
    /// <summary>
    /// Registra el mediator. Usa TryAdd: si cada módulo lo llama, solo se registra una vez.
    /// Scoped = una instancia por petición HTTP (igual que el DbContext que usarán los handlers).
    /// </summary>
    public static IServiceCollection AddMessaging(this IServiceCollection services)
    {
        services.TryAddScoped<IMediator, Mediator>();
        return services;
    }

    /// <summary>
    /// Busca en el ensamblado todas las clases que implementan ICommandHandler, IQueryHandler o
    /// IIntegrationEventHandler y las registra. Así no hay que añadir una línea por cada handler nuevo.
    ///
    /// Ejemplo: CreateShipmentHandler implementa ICommandHandler&lt;CreateShipmentCommand, CreateShipmentResult&gt;
    ///   → services.AddScoped(typeof(ICommandHandler&lt;CreateShipmentCommand, CreateShipmentResult&gt;), typeof(CreateShipmentHandler))
    ///
    /// Una clase puede implementar varias interfaces (ej. un handler para cinco eventos distintos):
    /// se registra una vez por cada interfaz.
    /// </summary>
    public static IServiceCollection AddHandlersFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        Type[] handlerInterfaces = [typeof(ICommandHandler<,>), typeof(IQueryHandler<,>), typeof(IIntegrationEventHandler<>)];

        var registrations =
            from type in assembly.GetTypes()
            where type is { IsClass: true, IsAbstract: false }
            from @interface in type.GetInterfaces()
            where @interface.IsGenericType
                  && handlerInterfaces.Contains(@interface.GetGenericTypeDefinition())
            select (Service: @interface, Implementation: type);

        foreach (var (service, implementation) in registrations)
            services.AddScoped(service, implementation);

        return services;
    }
}
