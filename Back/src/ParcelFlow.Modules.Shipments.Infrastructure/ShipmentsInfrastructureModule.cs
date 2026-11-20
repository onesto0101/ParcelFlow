using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using ParcelFlow.Modules.Shipments.Application.Abstractions;
using ParcelFlow.Modules.Shipments.Infrastructure.Persistence;

namespace ParcelFlow.Modules.Shipments.Infrastructure;

/// <summary>
/// Registro del módulo. La API llamará a services.AddShipmentsInfrastructure(connectionString)
/// y aquí se decide qué clase concreta responde a cada interfaz de Application.
/// </summary>
public static class ShipmentsInfrastructureModule
{
    public static IServiceCollection AddShipmentsInfrastructure(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<ShipmentsDbContext>(options => ConfigureDbContext(options, connectionString));

        // Mismo objeto para las dos interfaces: el repositorio añade al DbContext y el unit of work
        // guarda ESE MISMO DbContext. Si fueran instancias distintas, SaveChanges no guardaría nada.
        services.AddScoped<IShipmentsUnitOfWork>(sp => sp.GetRequiredService<ShipmentsDbContext>());
        services.AddScoped<IShipmentRepository, ShipmentRepository>();

        return services;
    }

    /// <summary>
    /// Aplica las migraciones pendientes del módulo. Pensado para desarrollo y tests.
    /// Se crea un "scope" porque el DbContext es Scoped (vive lo que dura una petición)
    /// y al arrancar la app todavía no hay ninguna petición.
    /// </summary>
    public static async Task MigrateShipmentsDatabaseAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ShipmentsDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    /// <summary>
    /// Configuración compartida por la app y por las herramientas de migraciones (dotnet ef).
    /// </summary>
    internal static void ConfigureDbContext(DbContextOptionsBuilder options, string connectionString) =>
        options.UseNpgsql(connectionString, npgsql =>
            // Cada módulo tiene su propio historial de migraciones dentro de su schema.
            // Si no, los tres módulos compartirían "public.__EFMigrationsHistory" y se pisarían.
            npgsql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, ShipmentsDbContext.Schema));
}
