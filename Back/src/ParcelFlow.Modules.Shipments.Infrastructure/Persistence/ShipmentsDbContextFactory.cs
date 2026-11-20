using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ParcelFlow.Modules.Shipments.Infrastructure.Persistence;

/// <summary>
/// Solo lo usa "dotnet ef" (migraciones) en tiempo de diseño, nunca la app en ejecución.
///
/// Sin esta clase, dotnet ef necesitaría arrancar la API para obtener el DbContext.
/// Con ella, las migraciones del módulo se generan desde este mismo proyecto.
/// </summary>
internal sealed class ShipmentsDbContextFactory : IDesignTimeDbContextFactory<ShipmentsDbContext>
{
    // Solo para desarrollo local (coincide con docker-compose.yml). Se puede sobreescribir
    // con la variable de entorno PARCELFLOW_DB.
    private const string LocalConnectionString =
        "Host=localhost;Port=5432;Database=parcelflow;Username=parcelflow;Password=parcelflow";

    public ShipmentsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("PARCELFLOW_DB") ?? LocalConnectionString;

        var options = new DbContextOptionsBuilder<ShipmentsDbContext>();
        ShipmentsInfrastructureModule.ConfigureDbContext(options, connectionString);

        return new ShipmentsDbContext(options.Options);
    }
}
