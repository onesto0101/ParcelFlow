using System.Security.Cryptography;

namespace ParcelFlow.Modules.Shipments.Domain;

/// <summary>
/// Número de guía: "PF" + 10 caracteres, por ejemplo <c>PF7K3M9QXR2A</c>.
///
/// El alfabeto excluye caracteres que se confunden al leerlos o dictarlos
/// (0/O, 1/I/L). Con 31 símbolos y 10 posiciones hay ~8×10^14 combinaciones,
/// así que una colisión es extremadamente improbable; aun así, la unicidad
/// definitiva la garantiza la base de datos con un índice único.
/// </summary>
public sealed record TrackingNumber
{
    public const string Prefix = "PF";
    public const int RandomPartLength = 10;
    public const int TotalLength = 12; // Prefix.Length + RandomPartLength
    public const string Alphabet = "23456789ABCDEFGHJKMNPQRSTUVWXYZ";

    private readonly string _value = string.Empty;

    public string Value
    {
        get => _value;
        init => _value = EnsureValid(value);
    }

    private TrackingNumber(string value)
    {
        Value = value;
    }

    /// <summary>Genera una guía nueva y aleatoria.</summary>
    public static TrackingNumber Generate()
    {
        var randomPart = RandomNumberGenerator.GetItems<char>(Alphabet, RandomPartLength);
        return new TrackingNumber(Prefix + new string(randomPart));
    }

    /// <summary>
    /// Reconstruye una guía a partir de texto (lo que escribe un usuario o lo que
    /// viene de la base de datos). Acepta minúsculas y espacios alrededor.
    /// </summary>
    public static TrackingNumber From(string value) => new(value);

    public override string ToString() => Value;

    private static string EnsureValid(string? value)
    {
        var normalized = value?.Trim().ToUpperInvariant() ?? string.Empty;

        var isValid =
            normalized.Length == TotalLength &&
            normalized.StartsWith(Prefix, StringComparison.Ordinal) &&
            normalized[Prefix.Length..].All(c => Alphabet.Contains(c));

        if (!isValid)
            throw new ArgumentException(
                $"Número de guía inválido: '{value}'. Formato esperado: {Prefix} + {RandomPartLength} caracteres.",
                nameof(Value));

        return normalized;
    }
}
