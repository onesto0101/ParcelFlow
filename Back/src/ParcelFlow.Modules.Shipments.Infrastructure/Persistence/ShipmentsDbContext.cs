using Microsoft.EntityFrameworkCore;
using ParcelFlow.Modules.Shipments.Application.Abstractions;
using ParcelFlow.Modules.Shipments.Domain;

namespace ParcelFlow.Modules.Shipments.Infrastructure.Persistence;

/// <summary>
/// La "sesión" con la base de datos del módulo Shipments.
///
/// - Un DbContext por módulo, y cada uno con su propio schema de Postgres ("shipments").
///   Así ningún módulo puede leer o escribir las tablas de otro: la frontera del módulo
///   también existe en la base de datos.
/// - También ES el unit of work: EF Core ya acumula todos los cambios y los guarda juntos,
///   en una transacción, al llamar a SaveChangesAsync.
/// </summary>
public sealed class ShipmentsDbContext(DbContextOptions<ShipmentsDbContext> options)
    : DbContext(options), IShipmentsUnitOfWork
{
    public const string Schema = "shipments";

    public DbSet<Shipment> Shipments => Set<Shipment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Todas las tablas de este DbContext van al schema "shipments" (no a "public").
        modelBuilder.HasDefaultSchema(Schema);

        // Busca todas las clases IEntityTypeConfiguration<T> de este proyecto y las aplica.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShipmentsDbContext).Assembly);
    }

    // Implementación explícita: DbContext.SaveChangesAsync devuelve Task<int> (filas afectadas)
    // y la interfaz pide Task. Application no necesita saber cuántas filas cambiaron.
    Task IShipmentsUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken) =>
        SaveChangesAsync(cancellationToken);
}
