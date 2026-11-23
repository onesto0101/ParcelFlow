using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using ParcelFlow.BuildingBlocks.Infrastructure.Outbox;
using ParcelFlow.Modules.Tracking.Application.Abstractions;
using ParcelFlow.Modules.Tracking.Infrastructure.Outbox;
using ParcelFlow.Modules.Tracking.Infrastructure.Persistence;

namespace ParcelFlow.Modules.Tracking.Infrastructure;

/// <summary>Registro del módulo Tracking (misma receta que ShipmentsInfrastructureModule).</summary>
public static class TrackingInfrastructureModule
{
    public static IServiceCollection AddTrackingInfrastructure(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<TrackingDbContext>((serviceProvider, options) =>
        {
            ConfigureDbContext(options, connectionString);
            options.AddOutboxInterceptor<TrackingDbContext>(serviceProvider, new TrackingDomainEventMapper());
        });

        services.AddScoped<ITrackingUnitOfWork>(sp => sp.GetRequiredService<TrackingDbContext>());
        services.AddScoped<IShipmentTrackingRepository, ShipmentTrackingRepository>();
        services.AddScoped<ITrackingQueries, TrackingQueries>();

        services.AddOutbox<TrackingDbContext>();

        return services;
    }

    public static async Task MigrateTrackingDatabaseAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TrackingDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    internal static void ConfigureDbContext(DbContextOptionsBuilder options, string connectionString) =>
        options.UseNpgsql(connectionString, npgsql =>
            npgsql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, TrackingDbContext.Schema));
}
