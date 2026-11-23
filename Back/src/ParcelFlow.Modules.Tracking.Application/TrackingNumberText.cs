namespace ParcelFlow.Modules.Tracking.Application;

/// <summary>
/// Tracking no conoce el value object TrackingNumber de Shipments (no puede referenciar su dominio),
/// así que solo normaliza el texto: sin espacios y en mayúsculas, como lo guarda ShipmentTracking.
/// </summary>
internal static class TrackingNumberText
{
    public static string Normalize(string? value)
    {
        var normalized = new string((value ?? string.Empty).Where(c => !char.IsWhiteSpace(c)).ToArray()).ToUpperInvariant();

        if (normalized.Length == 0)
            throw new ArgumentException("El número de guía es obligatorio.", "TrackingNumber");

        return normalized;
    }
}
