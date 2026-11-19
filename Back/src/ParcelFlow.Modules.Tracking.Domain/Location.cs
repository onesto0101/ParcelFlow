using ParcelFlow.BuildingBlocks.Domain;

namespace ParcelFlow.Modules.Tracking.Domain;

/// <summary>
/// Dónde ocurrió un movimiento (ej. "CEDIS Monterrey", "Guadalajara").
/// Fuera de alcance: coordenadas GPS y rutas reales.
/// </summary>
public sealed record Location
{
    public const int MinLength = 2;
    public const int MaxLength = 150;

    private readonly string _name = string.Empty;

    public string Name
    {
        get => _name;
        init => _name = Guard.RequiredText(value, nameof(Name), MaxLength, MinLength);
    }

    public Location(string name)
    {
        Name = name;
    }

    public override string ToString() => Name;
}
