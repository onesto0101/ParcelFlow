using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParcelFlow.Modules.Tracking.Domain;

namespace ParcelFlow.Modules.Tracking.Infrastructure.Persistence.Configurations;

/// <summary>
/// Un aggregate con una COLECCIÓN de entidades hijas → dos tablas:
///
///   tracking.ShipmentTrackings  (1)  ──<  tracking.TrackingEvents  (N)
///
/// A diferencia de Shipments, aquí los hijos (TrackingEvent) sí tienen identidad propia,
/// así que no se "aplanan" en columnas: van en su propia tabla, con clave foránea al padre.
/// </summary>
internal sealed class ShipmentTrackingConfiguration : IEntityTypeConfiguration<ShipmentTracking>
{
    public void Configure(EntityTypeBuilder<ShipmentTracking> builder)
    {
        builder.ToTable("ShipmentTrackings");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        // Un seguimiento por envío. Además de buscar rápido, es la red de seguridad de la
        // idempotencia: si ShipmentCreated se procesara dos veces a la vez, el segundo INSERT falla.
        builder.HasIndex(t => t.ShipmentId).IsUnique();

        builder.Property(t => t.TrackingNumber).HasMaxLength(ShipmentTracking.TrackingNumberMaxLength);
        builder.HasIndex(t => t.TrackingNumber).IsUnique();

        builder.Property(t => t.CurrentStatus).HasConversion<string>().HasMaxLength(30);

        // Value object de un solo valor → una columna de texto (value converter, como TrackingNumber en Shipments).
        builder.Property(t => t.CurrentLocation)
            .HasConversion(location => location.Name, name => new Location(name))
            .HasMaxLength(Location.MaxLength);

        // La colección: EF escribe directamente en el campo privado "_events" (lo encuentra por
        // convención) porque la propiedad Events es de solo lectura. El dominio sigue protegido.
        builder.HasMany(t => t.Events)
            .WithOne()
            .HasForeignKey("ShipmentTrackingId") // "shadow": la FK existe en la tabla, no en la clase
            .IsRequired() // un movimiento siempre pertenece a un seguimiento (NOT NULL)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(t => t.Events).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(t => t.DomainEvents);

        // Concurrencia optimista (ver ShipmentConfiguration). Aquí importa de verdad: dos operadores
        // registrando movimientos a la vez podrían saltarse la máquina de estados.
        builder.Property<uint>("xmin").IsRowVersion();
    }
}

internal sealed class TrackingEventConfiguration : IEntityTypeConfiguration<TrackingEvent>
{
    public void Configure(EntityTypeBuilder<TrackingEvent> builder)
    {
        builder.ToTable("TrackingEvents");
        builder.HasKey(e => e.Id);

        // ⚠️ Importante con hijos de Id generado por el dominio: si EF creyera que el Id lo genera
        // la BD, al ver un TrackingEvent nuevo CON Id pensaría "ya existe" y haría UPDATE en vez
        // de INSERT (y fallaría). ValueGeneratedNever = "si no lo conozco, es nuevo".
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(30);

        builder.Property(e => e.Location)
            .HasConversion(location => location.Name, name => new Location(name))
            .HasMaxLength(Location.MaxLength);

        builder.Property(e => e.Description)
            .HasConversion(description => description.Value, value => new TrackingDescription(value))
            .HasMaxLength(TrackingDescription.MaxLength);

        builder.HasIndex("ShipmentTrackingId", nameof(TrackingEvent.OccurredAt));
    }
}
