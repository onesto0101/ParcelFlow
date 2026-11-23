namespace ParcelFlow.BuildingBlocks.Application.Parsing;

/// <summary>
/// Convierte el texto que llega por HTTP ("InTransit", "intransit") en un enum del dominio.
///
/// Enum.TryParse por sí solo acepta números ("3") y valores no definidos ("99"),
/// así que aquí se exige un nombre real. Si no lo es: ArgumentException → la API responde 400.
/// </summary>
public static class EnumParsing
{
    public static TEnum Parse<TEnum>(string? value, string paramName)
        where TEnum : struct, Enum
    {
        var text = value?.Trim();

        if (string.IsNullOrEmpty(text)
            || char.IsAsciiDigit(text[0]) || text[0] == '-'
            || !Enum.TryParse<TEnum>(text, ignoreCase: true, out var result)
            || !Enum.IsDefined(result))
        {
            throw new ArgumentException(
                $"Valor inválido '{value}'. Valores permitidos: {string.Join(", ", Enum.GetNames<TEnum>())}.",
                paramName);
        }

        return result;
    }

    /// <summary>Igual que <see cref="Parse{TEnum}"/>, pero null/vacío significa "sin filtro".</summary>
    public static TEnum? ParseOptional<TEnum>(string? value, string paramName)
        where TEnum : struct, Enum =>
        string.IsNullOrWhiteSpace(value) ? null : Parse<TEnum>(value, paramName);
}
