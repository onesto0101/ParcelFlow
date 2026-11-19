namespace ParcelFlow.Modules.Shipments.Domain;

/// <summary>
/// Estado del envío desde el punto de vista de Shipments.
///
/// Es intencionadamente más simple que el estado de Tracking (PickedUp, InTransit...):
/// a Shipments solo le importa lo que afecta a SUS reglas (¿se puede cancelar?).
/// Cada bounded context modela solo lo que necesita.
/// </summary>
public enum ShipmentStatus
{
    Registered = 1,
    Delivered = 2,
    Cancelled = 3,
}
