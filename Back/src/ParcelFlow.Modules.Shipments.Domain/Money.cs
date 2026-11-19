namespace ParcelFlow.Modules.Shipments.Domain;

/// <summary>
/// Cantidad de dinero en una moneda concreta. Nunca existe un importe "sin moneda".
/// </summary>
public sealed record Money
{
    public const int DecimalPlaces = 2;

    private readonly decimal _amount;
    private readonly string _currency = string.Empty;

    /// <summary>Importe, siempre redondeado a 2 decimales y nunca negativo.</summary>
    public decimal Amount
    {
        get => _amount;
        init
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(Amount), value, "El importe no puede ser negativo.");

            _amount = Math.Round(value, DecimalPlaces, MidpointRounding.AwayFromZero);
        }
    }

    /// <summary>Código ISO 4217 de 3 letras (MXN, USD, EUR), siempre en mayúsculas.</summary>
    public string Currency
    {
        get => _currency;
        init
        {
            if (string.IsNullOrWhiteSpace(value) || value.Trim().Length != 3 || !value.Trim().All(char.IsAsciiLetter))
                throw new ArgumentException("La moneda debe ser un código ISO de 3 letras (ej. MXN).", nameof(Currency));

            _currency = value.Trim().ToUpperInvariant();
        }
    }

    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Zero(string currency) => new(0m, currency);

    /// <summary>Suma dos importes. Solo se permite con la misma moneda.</summary>
    public Money Add(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);
        EnsureSameCurrency(other);

        return new Money(Amount + other.Amount, Currency);
    }

    /// <summary>Multiplica por un factor (ej. kilos × tarifa por kilo).</summary>
    public Money Multiply(decimal factor)
    {
        if (factor < 0)
            throw new ArgumentOutOfRangeException(nameof(factor), factor, "El factor no puede ser negativo.");

        return new Money(Amount * factor, Currency);
    }

    public static Money operator +(Money left, Money right) => left.Add(right);

    public static Money operator *(Money money, decimal factor) => money.Multiply(factor);

    public override string ToString() =>
        string.Create(System.Globalization.CultureInfo.InvariantCulture, $"{Amount:0.00} {Currency}");

    private void EnsureSameCurrency(Money other)
    {
        if (other.Currency != Currency)
            throw new InvalidOperationException(
                $"No se pueden operar importes en monedas distintas ({Currency} y {other.Currency}).");
    }
}
