using Microsoft.Extensions.DependencyInjection;
using ParcelFlow.BuildingBlocks.Application.Messaging;

namespace ParcelFlow.Modules.Shipments.Application;

/// <summary>
/// Punto de registro del módulo. La API solo llamará a services.AddShipmentsApplication()
/// y no necesita conocer ningún handler (que además son internal).
/// </summary>
public static class ShipmentsApplicationModule
{
    public static IServiceCollection AddShipmentsApplication(this IServiceCollection services)
    {
        services.AddMessaging();
        services.AddHandlersFromAssembly(typeof(ShipmentsApplicationModule).Assembly);
        return services;
    }
}
