using ParcelFlow.BuildingBlocks.Domain;

namespace ParcelFlow.Modules.Tracking.Domain;

/// <summary>
/// Aggregate Root del módulo Tracking: el seguimiento de UN envío y su historial.
///
/// Fíjate en que no usa las clases de Shipments (ni Shipment, ni TrackingNumber):
/// un módulo no conoce el dominio de otro. Solo guarda el Id del envío y la guía
/// como texto, que es lo que le llegará en el evento ShipmentCreated.
/// </summary>
public sealed class ShipmentTracking : AggregateRoot<Guid>
{
    public const int TrackingNumberMaxLength = 20;

    private readonly List<TrackingEvent> _events = [];

    public Guid ShipmentId { get; private set; }
    public string TrackingNumber { get; private set; } = string.Empty;
    public TrackingStatus CurrentStatus { get; private set; }
    public Location CurrentLocation { get; private set; } = null!;
    public DateTime LastUpdatedAt { get; private set; }

    /// <summary>
    /// Historial en orden cronológico. Se expone como solo lectura: desde fuera
    /// nadie puede añadir, quitar ni reordenar movimientos.
    /// </summary>
    public IReadOnlyList<TrackingEvent> Events => _events.AsReadOnly();

    private ShipmentTracking() { }

    /// <summary>
    /// Inicia el seguimiento de un envío recién creado, con un primer movimiento "Created".
    /// </summary>
    public static ShipmentTracking Start(Guid shipmentId, string trackingNumber, Location origin, DateTime createdAt)
    {
        if (shipmentId == Guid.Empty)
            throw new ArgumentException("El Id del envío es obligatorio.", nameof(shipmentId));

        ArgumentNullException.ThrowIfNull(origin);

        var tracking = new ShipmentTracking
        {
            Id = Guid.CreateVersion7(),
            ShipmentId = shipmentId,
            TrackingNumber = Guard.RequiredText(trackingNumber, nameof(trackingNumber), TrackingNumberMaxLength).ToUpperInvariant(),
            CurrentStatus = TrackingStatus.Created,
            CurrentLocation = origin,
            LastUpdatedAt = createdAt,
        };

        tracking._events.Add(new TrackingEvent(
            TrackingStatus.Created, origin, new TrackingDescription("Envío registrado."), createdAt));

        return tracking;
    }

    public bool CanTransitionTo(TrackingStatus status) =>
        TrackingStatusTransitions.CanTransition(CurrentStatus, status);

    /// <summary>
    /// Registra un movimiento (TR-001, TR-004…TR-008). Cada cambio de estado deja
    /// un movimiento en el historial: el historial nunca se borra ni se reescribe.
    /// </summary>
    public void RegisterEvent(TrackingStatus status, Location location, TrackingDescription description, DateTime occurredAt)
    {
        ArgumentNullException.ThrowIfNull(location);
        ArgumentNullException.ThrowIfNull(description);

        if (!CanTransitionTo(status))
            throw new DomainException(
                "tracking.invalid_transition",
                $"No se puede pasar de {CurrentStatus} a {status}.");

        if (occurredAt < LastUpdatedAt)
            throw new DomainException(
                "tracking.event_out_of_order",
                "Un movimiento no puede ser anterior al último registrado.");

        var previousStatus = CurrentStatus;

        _events.Add(new TrackingEvent(status, location, description, occurredAt));
        CurrentStatus = status;
        CurrentLocation = location;
        LastUpdatedAt = occurredAt;

        RaiseDomainEvent(new ShipmentStatusChangedDomainEvent(
            Id, ShipmentId, TrackingNumber, previousStatus, status, location.Name, occurredAt));

        if (status == TrackingStatus.AtDistributionCenter)
            RaiseDomainEvent(new ShipmentArrivedAtCenterDomainEvent(
                Id, ShipmentId, TrackingNumber, location.Name, occurredAt));
    }
}
