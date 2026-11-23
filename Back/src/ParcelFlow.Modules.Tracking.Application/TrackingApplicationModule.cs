using Microsoft.Extensions.DependencyInjection;
using ParcelFlow.BuildingBlocks.Application.Messaging;

namespace ParcelFlow.Modules.Tracking.Application;

/// <summary>Punto de registro del módulo (igual que ShipmentsApplicationModule).</summary>
public static class TrackingApplicationModule
{
    public static IServiceCollection AddTrackingApplication(this IServiceCollection services)
    {
        services.AddMessaging();
        services.AddHandlersFromAssembly(typeof(TrackingApplicationModule).Assembly);
        return services;
    }
}
