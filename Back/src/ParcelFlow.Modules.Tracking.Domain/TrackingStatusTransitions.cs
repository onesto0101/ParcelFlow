namespace ParcelFlow.Modules.Tracking.Domain;

/// <summary>
/// Máquina de estados del seguimiento: qué estado puede ir después de cuál.
///
/// <code>
/// Created ─► PickedUp ─► InTransit ─► AtDistributionCenter ─► OutForDelivery ─► Delivered
///                            ▲                │                   │    ▲
///                            └────────────────┘                   ▼    │
///                     (puede pasar por varios centros)      DeliveryFailed
///
/// Cualquier estado NO final ─► Cancelled
/// Finales: Delivered, Cancelled
/// </code>
///
/// Está separada del aggregate para poder probarla sola y, más adelante, exponerla
/// (por ejemplo, para que el front sepa qué botones mostrar).
/// </summary>
public static class TrackingStatusTransitions
{
    private static readonly Dictionary<TrackingStatus, TrackingStatus[]> Allowed = new()
    {
        [TrackingStatus.Created] = [TrackingStatus.PickedUp, TrackingStatus.Cancelled],
        [TrackingStatus.PickedUp] = [TrackingStatus.InTransit, TrackingStatus.Cancelled],
        [TrackingStatus.InTransit] = [TrackingStatus.AtDistributionCenter, TrackingStatus.Cancelled],
        [TrackingStatus.AtDistributionCenter] = [TrackingStatus.InTransit, TrackingStatus.OutForDelivery, TrackingStatus.Cancelled],
        [TrackingStatus.OutForDelivery] = [TrackingStatus.Delivered, TrackingStatus.DeliveryFailed, TrackingStatus.Cancelled],
        [TrackingStatus.DeliveryFailed] = [TrackingStatus.OutForDelivery, TrackingStatus.Cancelled],
        [TrackingStatus.Delivered] = [],
        [TrackingStatus.Cancelled] = [],
    };

    public static bool CanTransition(TrackingStatus from, TrackingStatus to) =>
        Allowed.TryGetValue(from, out var targets) && targets.Contains(to);

    public static IReadOnlyList<TrackingStatus> NextStatuses(TrackingStatus from) =>
        Allowed.TryGetValue(from, out var targets) ? targets : [];

    public static bool IsFinal(TrackingStatus status) => NextStatuses(status).Count == 0;
}
