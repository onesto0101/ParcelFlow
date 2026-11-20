using Microsoft.EntityFrameworkCore;
using ParcelFlow.Modules.Shipments.Infrastructure;
using ParcelFlow.Modules.Shipments.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace ParcelFlow.Modules.Shipments.IntegrationTests;

/// <summary>
/// Levanta un Postgres REAL y desechable en Docker para los tests (Testcontainers).
/// Se crea una vez para toda la clase de tests (IClassFixture) y se destruye al final.
///
/// ¿Por qué no una BD en memoria? Porque queremos probar lo que hará Postgres de verdad:
/// tipos de columna, índice único, schema, migraciones. Un fake no detectaría esos errores.
/// Requisito: Docker Desktop encendido.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18").Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        // Aplica las mismas migraciones que usará producción (no EnsureCreated).
        await using var db = CreateDbContext();
        await db.Database.MigrateAsync();
    }

    /// <summary>
    /// Cada llamada da un DbContext NUEVO (sin nada en memoria). Así, al leer, los datos vienen
    /// de verdad de Postgres y no de la caché del DbContext que los guardó.
    /// </summary>
    public ShipmentsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ShipmentsDbContext>();
        ShipmentsInfrastructureModule.ConfigureDbContext(options, _container.GetConnectionString());
        return new ShipmentsDbContext(options.Options);
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}
