using ParcelFlow.BuildingBlocks.Domain;

namespace ParcelFlow.Modules.Tracking.Domain;

/// <summary>Texto que explica un movimiento (ej. "Recibido en almacén").</summary>
public sealed record TrackingDescription
{
    public const int MaxLength = 500;

    private readonly string _value = string.Empty;

    public string Value
    {
        get => _value;
        init => _value = Guard.RequiredText(value, nameof(Value), MaxLength);
    }

    public TrackingDescription(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;
}
