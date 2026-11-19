namespace ParcelFlow.Modules.Shipments.Domain;

public sealed record Dimensions
{
    public const decimal MinCentimeters = 10m;   // inclusivo: 10 está permitido
    public const decimal MaxCentimeters = 150m;  // inclusivo: 150 está permitido

    // cm³ por kg. Estándar habitual en paquetería; algunas empresas usan 6000.
    public const decimal VolumetricDivisor = 5000m;

    private readonly decimal _length;
    private readonly decimal _width;
    private readonly decimal _height;

    public decimal Length
    {
        get => _length;
        init => _length = EnsureValid(value, nameof(Length));
    }

    public decimal Width
    {
        get => _width;
        init => _width = EnsureValid(value, nameof(Width));
    }

    public decimal Height
    {
        get => _height;
        init => _height = EnsureValid(value, nameof(Height));
    }

    public Dimensions(decimal length, decimal width, decimal height)
    {
        Length = length;
        Width = width;
        Height = height;
    }

    /// <summary>Volumen en cm³.</summary>
    public decimal Volume => Length * Width * Height;

    /// <summary>
    /// Peso volumétrico en kg. Puede superar <see cref="Weight.MaxKilograms"/>:
    /// es un dato para calcular tarifas, no el peso físico del paquete.
    /// </summary>
    public decimal VolumetricWeight => Volume / VolumetricDivisor;

    private static decimal EnsureValid(decimal value, string propertyName)
    {
        if (value < MinCentimeters || value > MaxCentimeters)
            throw new ArgumentOutOfRangeException(
                propertyName,
                value,
                $"{propertyName} debe estar entre {MinCentimeters} y {MaxCentimeters} cm."
            );

        return value;
    }
}
