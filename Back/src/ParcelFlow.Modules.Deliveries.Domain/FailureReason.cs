using ParcelFlow.BuildingBlocks.Domain;

namespace ParcelFlow.Modules.Deliveries.Domain;

public enum DeliveryFailureCode
{
    RecipientAbsent = 1,
    WrongAddress = 2,
    Refused = 3,
    Inaccessible = 4,
    Other = 5,
}

/// <summary>
/// Motivo de una entrega fallida.
///
/// ⚠️ Aquí hay una regla que mira DOS propiedades a la vez: "si el código es Other,
/// los detalles son obligatorios". Validar en el init (como en Weight) no sirve:
/// cuando se ejecuta el init de Code, Details todavía no tiene valor.
///
/// Solución: propiedades de solo lectura ({ get; } sin init) asignadas en el
/// constructor, que valida todo junto. Al no tener init, la expresión
/// <c>reason with { Code = ... }</c> ni siquiera compila: la única forma de crear
/// un FailureReason es el constructor, y el constructor siempre valida.
/// </summary>
public sealed record FailureReason
{
    public const int DetailsMaxLength = 500;

    public DeliveryFailureCode Code { get; }
    public string? Details { get; }

    public FailureReason(DeliveryFailureCode code, string? details = null)
    {
        if (!Enum.IsDefined(code))
            throw new ArgumentOutOfRangeException(nameof(code), code, "Código de fallo desconocido.");

        var normalizedDetails = Guard.OptionalText(details, nameof(details), DetailsMaxLength);

        if (code == DeliveryFailureCode.Other && normalizedDetails is null)
            throw new ArgumentException("Si el motivo es 'Other', hay que explicar los detalles.", nameof(details));

        Code = code;
        Details = normalizedDetails;
    }
}
