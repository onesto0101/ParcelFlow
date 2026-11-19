namespace ParcelFlow.Modules.Deliveries.Domain;

/// <summary>
/// <code>
/// Pending ─► Assigned ─► Delivered
///    ▲          │
///    └──────────┘  intento fallido (si quedan intentos: vuelve a Pending para reasignar)
///               │
///               └─► Failed      (3er intento fallido)
///
/// Pending / Assigned ─► Cancelled (el envío se canceló)
/// </code>
/// </summary>
public enum DeliveryStatus
{
    Pending = 1,
    Assigned = 2,
    Delivered = 3,
    Failed = 4,
    Cancelled = 5,
}
