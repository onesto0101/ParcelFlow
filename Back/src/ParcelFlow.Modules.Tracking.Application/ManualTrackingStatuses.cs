using ParcelFlow.Modules.Tracking.Domain;

namespace ParcelFlow.Modules.Tracking.Application;

/// <summary>
/// Qué estados puede registrar un OPERADOR a mano (TR-004…TR-006). El resto llegan solos,
/// por eventos de otros módulos:
///
///   Created          ← Shipments (ShipmentCreated)
///   OutForDelivery   ← Deliveries (DeliveryAssigned)
///   DeliveryFailed   ← Deliveries (DeliveryAttemptFailed)
///   Delivered        ← Deliveries (ShipmentDelivered)
///   Cancelled        ← Shipments (ShipmentCancelled)
///
/// Si el operador pudiera marcar "Delivered" desde Rastreo, Tracking diría "entregado" y Deliveries
/// diría "pendiente": dos verdades. Cada hecho tiene UN dueño.
///
/// Es una regla de la aplicación (quién puede hacer qué), no del dominio: al aggregate le da igual
/// quién registra el movimiento, solo le importa que la transición sea válida.
/// </summary>
public static class ManualTrackingStatuses
{
    public static readonly IReadOnlyList<TrackingStatus> All =
    [
        TrackingStatus.PickedUp,
        TrackingStatus.InTransit,
        TrackingStatus.AtDistributionCenter,
    ];

    public static bool IsManual(TrackingStatus status) => All.Contains(status);

    /// <summary>Los estados manuales a los que se puede pasar desde el actual (para los botones del front).</summary>
    public static IReadOnlyList<TrackingStatus> NextFrom(TrackingStatus current) =>
        TrackingStatusTransitions.NextStatuses(current).Where(IsManual).ToList();
}
