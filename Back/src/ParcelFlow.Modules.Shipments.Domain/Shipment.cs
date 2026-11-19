using ParcelFlow.BuildingBlocks.Domain;

namespace ParcelFlow.Modules.Shipments.Domain;

/// <summary>
/// Aggregate Root del módulo Shipments.
///
/// Todo cambio de un envío pasa por sus métodos (Create, Cancel, MarkAsDelivered),
/// que son los que protegen las reglas de negocio. No hay setters públicos.
///
/// Las fechas se reciben como parámetro (no se usa DateTime.UtcNow aquí dentro):
/// así el dominio es determinista y los tests pueden usar la fecha que quieran.
/// Convención: todas las fechas son UTC.
/// </summary>
public sealed class Shipment : AggregateRoot<Guid>
{
    public const int CancellationReasonMaxLength = 500;

    public TrackingNumber TrackingNumber { get; private set; } = null!;
    public Contact Sender { get; private set; } = null!;
    public Contact Recipient { get; private set; } = null!;
    public Weight Weight { get; private set; } = null!;
    public Dimensions Dimensions { get; private set; } = null!;
    public Money Cost { get; private set; } = null!;
    public ShipmentStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTime? DeliveredAt { get; private set; }

    // Constructor vacío y privado: nadie puede hacer "new Shipment()" desde fuera.
    // (Más adelante EF Core también lo usará para reconstruir el objeto desde la BD.)
    private Shipment() { }

    /// <summary>
    /// Registra un envío nuevo (SH-001…SH-007). El costo lo calcula el dominio,
    /// no se recibe desde fuera: así nadie puede crear un envío con un precio inventado.
    /// </summary>
    public static Shipment Create(
        TrackingNumber trackingNumber,
        Contact sender,
        Contact recipient,
        Weight weight,
        Dimensions dimensions,
        DateTime createdAt)
    {
        ArgumentNullException.ThrowIfNull(trackingNumber);
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentNullException.ThrowIfNull(recipient);
        ArgumentNullException.ThrowIfNull(weight);
        ArgumentNullException.ThrowIfNull(dimensions);

        var shipment = new Shipment
        {
            // Guid v7: incluye la fecha, así los Ids salen ordenados en el tiempo
            // (mejor rendimiento en índices de base de datos que un Guid v4 aleatorio).
            Id = Guid.CreateVersion7(),
            TrackingNumber = trackingNumber,
            Sender = sender,
            Recipient = recipient,
            Weight = weight,
            Dimensions = dimensions,
            Cost = ShippingCostCalculator.Calculate(weight, dimensions),
            Status = ShipmentStatus.Registered,
            CreatedAt = createdAt,
        };

        shipment.RaiseDomainEvent(new ShipmentCreatedDomainEvent(
            shipment.Id,
            trackingNumber.Value,
            sender.Address.City,
            recipient.Address.City,
            createdAt));

        return shipment;
    }

    /// <summary>
    /// Cancela el envío (SH-010). No se borra nada: se conserva el envío con su
    /// fecha y motivo de cancelación ("la cancelación debe conservar el historial").
    /// </summary>
    public void Cancel(string reason, DateTime cancelledAt)
    {
        if (Status == ShipmentStatus.Delivered)
            throw new DomainException("shipment.already_delivered", "Un envío entregado no puede cancelarse.");

        if (Status == ShipmentStatus.Cancelled)
            throw new DomainException("shipment.already_cancelled", "El envío ya está cancelado.");

        CancellationReason = Guard.RequiredText(reason, nameof(reason), CancellationReasonMaxLength);
        CancelledAt = cancelledAt;
        Status = ShipmentStatus.Cancelled;

        RaiseDomainEvent(new ShipmentCancelledDomainEvent(Id, TrackingNumber.Value, CancellationReason, cancelledAt));
    }

    /// <summary>
    /// Marca el envío como entregado. Lo invocará el módulo Deliveries (vía evento)
    /// cuando se confirme la entrega.
    ///
    /// Es idempotente: si llega dos veces el mismo aviso, la segunda no hace nada.
    /// Los consumidores de eventos deben ser idempotentes porque un evento puede repetirse.
    /// </summary>
    public void MarkAsDelivered(DateTime deliveredAt)
    {
        if (Status == ShipmentStatus.Delivered)
            return;

        if (Status == ShipmentStatus.Cancelled)
            throw new DomainException("shipment.cancelled", "Un envío cancelado no puede volver a procesarse.");

        Status = ShipmentStatus.Delivered;
        DeliveredAt = deliveredAt;
    }
}
