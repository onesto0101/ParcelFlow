using ParcelFlow.BuildingBlocks.Domain;

namespace ParcelFlow.Modules.Tracking.Domain;

/// <summary>
/// Un movimiento del historial. Es una ENTIDAD (tiene Id): dos movimientos con el mismo
/// estado, lugar y hora siguen siendo dos movimientos distintos.
///
/// Es inmutable ("los movimientos históricos son inmutables"): no tiene ningún método
/// que lo modifique, y su constructor es internal para que solo el aggregate
/// <see cref="ShipmentTracking"/> pueda crearlo.
/// </summary>
public sealed class TrackingEvent : Entity<Guid>
{
    public TrackingStatus Status { get; private set; }
    public Location Location { get; private set; } = null!;
    public TrackingDescription Description { get; private set; } = null!;
    public DateTime OccurredAt { get; private set; }

    private TrackingEvent() { }

    internal TrackingEvent(TrackingStatus status, Location location, TrackingDescription description, DateTime occurredAt)
    {
        Id = Guid.CreateVersion7();
        Status = status;
        Location = location;
        Description = description;
        OccurredAt = occurredAt;
    }
}
