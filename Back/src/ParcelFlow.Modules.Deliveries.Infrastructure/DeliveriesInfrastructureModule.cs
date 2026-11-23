using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using ParcelFlow.BuildingBlocks.Infrastructure.Outbox;
using ParcelFlow.Modules.Deliveries.Application.Abstractions;
using ParcelFlow.Modules.Deliveries.Infrastructure.Outbox;
using ParcelFlow.Modules.Deliveries.Infrastructure.Persistence;

namespace ParcelFlow.Modules.Deliveries.Infrastructure;

/// <summary>Registro del módulo Deliveries (misma receta que ShipmentsInfrastructureModule).</summary>
public static class DeliveriesInfrastructureModule
{
    public static IServiceCollection AddDeliveriesInfrastructure(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<DeliveriesDbContext>((serviceProvider, options) =>
        {
            ConfigureDbContext(options, connectionString);
            options.AddOutboxInterceptor<DeliveriesDbContext>(serviceProvider, new DeliveriesDomainEventMapper());
        });

        services.AddScoped<IDeliveriesUnitOfWork>(sp => sp.GetRequiredService<DeliveriesDbContext>());
        services.AddScoped<IDeliveryRepository, DeliveryRepository>();
        services.AddScoped<IDeliveryQueries, DeliveryQueries>();

        services.AddOutbox<DeliveriesDbContext>();

        return services;
    }

    public static async Task MigrateDeliveriesDatabaseAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DeliveriesDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    internal static void ConfigureDbContext(DbContextOptionsBuilder options, string connectionString) =>
        options.UseNpgsql(connectionString, npgsql =>
            npgsql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, DeliveriesDbContext.Schema));
}
