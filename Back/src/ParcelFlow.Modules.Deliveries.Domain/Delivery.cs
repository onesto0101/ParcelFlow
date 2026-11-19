using ParcelFlow.BuildingBlocks.Domain;

namespace ParcelFlow.Modules.Deliveries.Domain;

/// <summary>
/// Aggregate Root del módulo Deliveries: la última milla de UN envío.
///
/// Reglas del diseño y dónde se cumplen:
/// - "Solo se asigna cuando está listo para reparto"   → una Delivery solo se CREA cuando
///                                                       Tracking avisa que llegó al centro.
/// - "No se confirma sin repartidor asignado"          → ConfirmDelivery exige Assigned.
/// - "Máximo tres intentos"                            → MaxAttempts + estado Failed.
/// - "Una fallida registra motivo"                     → ReportFailure exige FailureReason.
/// - "Una exitosa registra fecha y receptor"           → ConfirmDelivery exige DeliveryEvidence.
/// - "Un entregado no vuelve a salir a reparto"        → AssignCourier rechaza Delivered.
/// </summary>
public sealed class Delivery : AggregateRoot<Guid>
{
    public const int MaxAttempts = 3;
    public const int TrackingNumberMaxLength = 20;

    private readonly List<DeliveryAttempt> _attempts = [];

    public Guid ShipmentId { get; private set; }
    public string TrackingNumber { get; private set; } = string.Empty;
    public DeliveryStatus Status { get; private set; }
    public CourierId? CourierId { get; private set; }
    public DateTime? AssignedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public IReadOnlyList<DeliveryAttempt> Attempts => _attempts.AsReadOnly();

    public int RemainingAttempts => MaxAttempts - _attempts.Count;

    private Delivery() { }

    /// <summary>Crea una entrega pendiente de asignar repartidor.</summary>
    public static Delivery CreatePending(Guid shipmentId, string trackingNumber, DateTime createdAt)
    {
        if (shipmentId == Guid.Empty)
            throw new ArgumentException("El Id del envío es obligatorio.", nameof(shipmentId));

        return new Delivery
        {
            Id = Guid.CreateVersion7(),
            ShipmentId = shipmentId,
            TrackingNumber = Guard.RequiredText(trackingNumber, nameof(trackingNumber), TrackingNumberMaxLength).ToUpperInvariant(),
            Status = DeliveryStatus.Pending,
            CreatedAt = createdAt,
        };
    }

    /// <summary>
    /// Asigna (o reasigna) un repartidor (DE-002). Se permite reasignar mientras no
    /// haya salido el intento, por ejemplo si el repartidor original se enferma.
    /// </summary>
    public void AssignCourier(CourierId courierId, DateTime assignedAt)
    {
        ArgumentNullException.ThrowIfNull(courierId);
        EnsureNotFinished();

        CourierId = courierId;
        AssignedAt = assignedAt;
        Status = DeliveryStatus.Assigned;

        RaiseDomainEvent(new DeliveryAssignedDomainEvent(Id, ShipmentId, TrackingNumber, courierId.Value, assignedAt));
    }

    /// <summary>Confirma la entrega exitosa (DE-005, DE-008).</summary>
    public void ConfirmDelivery(DeliveryEvidence evidence, DateTime deliveredAt)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        EnsureAssigned();

        _attempts.Add(DeliveryAttempt.Successful(_attempts.Count + 1, CourierId!, evidence, deliveredAt));
        Status = DeliveryStatus.Delivered;
        CompletedAt = deliveredAt;

        RaiseDomainEvent(new ShipmentDeliveredDomainEvent(Id, ShipmentId, TrackingNumber, evidence.ReceivedBy, deliveredAt));
    }

    /// <summary>
    /// Registra un intento fallido (DE-004, DE-006, DE-007).
    /// - Si quedan intentos: vuelve a Pending y hay que asignar repartidor otra vez
    ///   (normalmente al día siguiente, en una nueva ruta).
    /// - Si era el último: la entrega queda Failed (estado final).
    /// </summary>
    public void ReportFailure(FailureReason reason, DateTime attemptedAt)
    {
        ArgumentNullException.ThrowIfNull(reason);
        EnsureAssigned();

        var attempt = DeliveryAttempt.Failed(_attempts.Count + 1, CourierId!, reason, attemptedAt);
        _attempts.Add(attempt);

        var isFinal = _attempts.Count >= MaxAttempts;

        if (isFinal)
        {
            Status = DeliveryStatus.Failed;
            CompletedAt = attemptedAt;
        }
        else
        {
            Status = DeliveryStatus.Pending;
        }

        CourierId = null;
        AssignedAt = null;

        RaiseDomainEvent(new DeliveryAttemptFailedDomainEvent(
            Id, ShipmentId, TrackingNumber, attempt.AttemptNumber, reason.Code, isFinal, attemptedAt));
    }

    /// <summary>
    /// Cancela la entrega porque el envío se canceló. Idempotente: si ya estaba
    /// cancelada o fallida, no hace nada.
    /// </summary>
    public void Cancel(DateTime cancelledAt)
    {
        if (Status is DeliveryStatus.Cancelled or DeliveryStatus.Failed)
            return;

        if (Status == DeliveryStatus.Delivered)
            throw new DomainException("delivery.already_delivered", "Una entrega completada no puede cancelarse.");

        Status = DeliveryStatus.Cancelled;
        CompletedAt = cancelledAt;
        CourierId = null;
        AssignedAt = null;

        RaiseDomainEvent(new DeliveryCancelledDomainEvent(Id, ShipmentId, TrackingNumber, cancelledAt));
    }

    private void EnsureNotFinished()
    {
        switch (Status)
        {
            case DeliveryStatus.Delivered:
                throw new DomainException("delivery.already_delivered", "Un paquete entregado no puede volver a salir a reparto.");
            case DeliveryStatus.Failed:
                throw new DomainException("delivery.max_attempts_reached", $"Se alcanzó el máximo de {MaxAttempts} intentos.");
            case DeliveryStatus.Cancelled:
                throw new DomainException("delivery.cancelled", "La entrega está cancelada.");
        }
    }

    private void EnsureAssigned()
    {
        EnsureNotFinished();

        if (Status != DeliveryStatus.Assigned || CourierId is null)
            throw new DomainException("delivery.not_assigned", "La entrega no tiene un repartidor asignado.");
    }
}
