using ParcelFlow.BuildingBlocks.Application.Messaging;

namespace ParcelFlow.Modules.Tracking.Application.RegisterTrackingEvent;

/// <summary>
/// TR-001 / TR-004…TR-008: un operador registra un movimiento del paquete
/// ("Recibido en CEDIS Monterrey", "Salió hacia Guadalajara"...).
/// </summary>
/// <param name="Status">PickedUp, InTransit o AtDistributionCenter (ver ManualTrackingStatuses).</param>
/// <param name="OccurredAt">Cuándo ocurrió (UTC). Null = ahora. Permite registrar algo que pasó hace un rato.</param>
public sealed record RegisterTrackingEventCommand(
    string TrackingNumber,
    string Status,
    string Location,
    string Description,
    DateTime? OccurredAt = null) : ICommand<Unit>;
