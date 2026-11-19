namespace ParcelFlow.BuildingBlocks.Domain;

/// <summary>
/// Validaciones repetidas en muchos value objects (regla de tres: Address, Contact,
/// Location, TrackingDescription, CourierId, DeliveryEvidence... todos validan texto).
/// </summary>
public static class Guard
{
    /// <summary>
    /// Exige un texto con contenido y devuelve su versión sin espacios al inicio/final.
    /// </summary>
    public static string RequiredText(string? value, string paramName, int maxLength, int minLength = 1)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{paramName} es obligatorio.", paramName);

        var trimmed = value.Trim();

        if (trimmed.Length < minLength || trimmed.Length > maxLength)
            throw new ArgumentException(
                $"{paramName} debe tener entre {minLength} y {maxLength} caracteres.",
                paramName);

        return trimmed;
    }

    /// <summary>
    /// Igual que <see cref="RequiredText"/> pero permite null/vacío (devuelve null).
    /// </summary>
    public static string? OptionalText(string? value, string paramName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return RequiredText(value, paramName, maxLength);
    }
}
