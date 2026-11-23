using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParcelFlow.Modules.Shipments.Domain;

namespace ParcelFlow.Modules.Shipments.Infrastructure.Persistence.Configurations;

/// <summary>
/// Cómo se guarda un Shipment en tablas. Todo el "conocimiento de base de datos" vive aquí,
/// fuera del dominio: el dominio no tiene ni un solo atributo [Column] ni [Key].
///
/// Resultado: UNA sola tabla "shipments.Shipments". Los value objects no tienen tabla propia:
/// se "aplanan" en columnas de la misma fila (ej. Sender_Address_City).
/// </summary>
internal sealed class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.HasKey(s => s.Id);

        // El Id lo genera el dominio (Guid v7), no la base de datos.
        builder.Property(s => s.Id).ValueGeneratedNever();

        // Value object de un solo valor → una columna de texto, mediante un "value converter".
        builder.Property(s => s.TrackingNumber)
            .HasConversion(
                trackingNumber => trackingNumber.Value,    // al guardar
                value => TrackingNumber.From(value))       // al leer
            .HasMaxLength(TrackingNumber.TotalLength);

        // La garantía final de que no hay dos guías iguales (ver comentario en TrackingNumber).
        builder.HasIndex(s => s.TrackingNumber).IsUnique();

        // Value objects de varios valores → "complex properties": varias columnas en la misma tabla.
        builder.ComplexProperty(s => s.Sender, ConfigureContact);
        builder.ComplexProperty(s => s.Recipient, ConfigureContact);

        builder.ComplexProperty(s => s.Weight, weight =>
            weight.Property(w => w.Kilograms).HasPrecision(6, 3));

        builder.ComplexProperty(s => s.Dimensions, dimensions =>
        {
            dimensions.Property(d => d.Length).HasPrecision(6, 2);
            dimensions.Property(d => d.Width).HasPrecision(6, 2);
            dimensions.Property(d => d.Height).HasPrecision(6, 2);
        });

        builder.ComplexProperty(s => s.Cost, cost =>
        {
            cost.Property(c => c.Amount).HasPrecision(12, Money.DecimalPlaces);
            cost.Property(c => c.Currency).HasMaxLength(3);
        });

        // Enum como texto ("Registered") y no como número (1): legible en la BD y no se rompe
        // si alguien reordena el enum.
        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(s => s.CancellationReason).HasMaxLength(Shipment.CancellationReasonMaxLength);

        // Los eventos de dominio son "mensajes en memoria", no datos: no se guardan.
        // (Los que deben salir del módulo se guardan aparte, en el outbox.)
        builder.Ignore(s => s.DomainEvents);

        // Concurrencia optimista con "xmin", una columna de sistema que Postgres cambia en cada UPDATE.
        // Si dos peticiones cargan el mismo envío y las dos lo modifican, la segunda falla con
        // DbUpdateConcurrencyException (→ 409) en vez de pisar en silencio el cambio de la primera.
        // Es una propiedad "shadow": existe para EF pero no en la clase Shipment (el dominio no la ve).
        builder.Property<uint>("xmin").IsRowVersion();
    }

    private static void ConfigureContact(ComplexPropertyBuilder<Contact> contact)
    {
        contact.Property(c => c.Name).HasMaxLength(Contact.NameMaxLength);
        contact.Property(c => c.Phone).HasMaxLength(Contact.PhoneMaxDigits + 1); // + el "+" opcional

        contact.ComplexProperty(c => c.Address, address =>
        {
            address.Property(a => a.Street).HasMaxLength(Address.StreetMaxLength);
            address.Property(a => a.City).HasMaxLength(Address.CityMaxLength);
            address.Property(a => a.State).HasMaxLength(Address.StateMaxLength);
            address.Property(a => a.PostalCode).HasMaxLength(Address.PostalCodeMaxLength);
            address.Property(a => a.Country).HasMaxLength(2);
        });
    }
}
