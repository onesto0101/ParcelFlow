namespace ParcelFlow.Modules.Shipments.Domain;

public sealed record Weight
{
    public const decimal MinKilograms = 0m;   // exclusivo: tiene que ser mayor que esto
    public const decimal MaxKilograms = 50m;  // inclusivo: 50 está permitido

    private decimal _kilograms;

    public decimal Kilograms
    {
        get => _kilograms;

        init
        {
            if (value <= MinKilograms || value > MaxKilograms)
                throw new ArgumentOutOfRangeException(
                    nameof(Kilograms),
                    value,
                    $"El peso debe ser mayor a {MinKilograms} y menor o igual a {MaxKilograms} kg."
                );

            _kilograms = value;
        }
    }

    public Weight(decimal kilograms)
    {
        Kilograms = kilograms;
    }
}
