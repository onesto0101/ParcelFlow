using ParcelFlow.BuildingBlocks.Domain;

namespace ParcelFlow.Modules.Deliveries.Domain;

/// <summary>
/// Identificador del repartidor (ej. "juan.perez" o "REP-014").
///
/// La gestión de repartidores está fuera del alcance de la v1, así que no hay un
/// aggregate Courier: solo un identificador validado. Usar un tipo propio en vez de
/// un string suelto evita confundirlo con otros textos (una guía, un nombre...).
/// </summary>
public sealed record CourierId
{
    public const int MinLength = 2;
    public const int MaxLength = 50;

    private readonly string _value = string.Empty;

    public string Value
    {
        get => _value;
        init => _value = Guard.RequiredText(value, nameof(Value), MaxLength, MinLength);
    }

    public CourierId(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;
}
