using Microsoft.Extensions.DependencyInjection;
using ParcelFlow.BuildingBlocks.Application.Messaging;

namespace ParcelFlow.Modules.Deliveries.Application;

/// <summary>Punto de registro del módulo (igual que ShipmentsApplicationModule).</summary>
public static class DeliveriesApplicationModule
{
    public static IServiceCollection AddDeliveriesApplication(this IServiceCollection services)
    {
        services.AddMessaging();
        services.AddHandlersFromAssembly(typeof(DeliveriesApplicationModule).Assembly);
        return services;
    }
}
