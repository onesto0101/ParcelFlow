using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParcelFlow.Modules.Deliveries.Domain;

namespace ParcelFlow.Modules.Deliveries.Infrastructure.Persistence.Configurations;

/// <summary>
///   deliveries.Deliveries  (1)  ──<  deliveries.DeliveryAttempts  (N, máximo 3)
/// </summary>
internal sealed class DeliveryConfiguration : IEntityTypeConfiguration<Delivery>
{
    public void Configure(EntityTypeBuilder<Delivery> builder)
    {
        builder.ToTable("Deliveries");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        // UNA entrega por envío: refuerza en la BD la idempotencia del handler de ShipmentReadyForDelivery.
        builder.HasIndex(d => d.ShipmentId).IsUnique();

        builder.Property(d => d.TrackingNumber).HasMaxLength(Delivery.TrackingNumberMaxLength);
        builder.HasIndex(d => d.TrackingNumber).IsUnique();

        builder.Property(d => d.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(d => d.Status); // la vista principal filtra por estado (pendientes)

        // Value object opcional de un solo valor. EF no llama al converter con null:
        // una columna NULL se lee como CourierId = null directamente.
        builder.Property(d => d.CourierId)
            .HasConversion(courier => courier!.Value, value => new CourierId(value))
            .HasMaxLength(CourierId.MaxLength);
        builder.HasIndex(d => d.CourierId); // DE-003: entregas de un repartidor

        builder.HasMany(d => d.Attempts)
            .WithOne()
            .HasForeignKey("DeliveryId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(d => d.Attempts).UsePropertyAccessMode(PropertyAccessMode.Field);

        // Propiedad calculada (MaxAttempts - intentos): no es una columna.
        builder.Ignore(d => d.RemainingAttempts);
        builder.Ignore(d => d.DomainEvents);

        builder.Property<uint>("xmin").IsRowVersion();
    }
}

internal sealed class DeliveryAttemptConfiguration : IEntityTypeConfiguration<DeliveryAttempt>
{
    public void Configure(EntityTypeBuilder<DeliveryAttempt> builder)
    {
        builder.ToTable("DeliveryAttempts");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever(); // ver el ⚠️ en TrackingEventConfiguration

        builder.Property(a => a.CourierId)
            .HasConversion(courier => courier.Value, value => new CourierId(value))
            .HasMaxLength(CourierId.MaxLength);

        // Dos value objects OPCIONALES en la misma fila: un intento tiene Evidence o FailureReason.
        // Complex property opcional: si todas sus columnas son NULL, EF lo lee como null.
        // Columnas: Evidence_ReceivedBy, Evidence_Notes, FailureReason_Code, FailureReason_Details.
        builder.ComplexProperty(a => a.Evidence, evidence =>
        {
            evidence.Property(e => e.ReceivedBy).HasMaxLength(DeliveryEvidence.ReceivedByMaxLength);
            evidence.Property(e => e.Notes).HasMaxLength(DeliveryEvidence.NotesMaxLength);
        });

        builder.ComplexProperty(a => a.FailureReason, reason =>
        {
            reason.Property(r => r.Code).HasConversion<string>().HasMaxLength(20);
            reason.Property(r => r.Details).HasMaxLength(FailureReason.DetailsMaxLength);
        });
    }
}
