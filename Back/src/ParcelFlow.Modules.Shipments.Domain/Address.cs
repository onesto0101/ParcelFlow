using ParcelFlow.BuildingBlocks.Domain;

namespace ParcelFlow.Modules.Shipments.Domain;

/// <summary>
/// Dirección postal. Value object: dos direcciones con los mismos datos son la misma dirección.
/// Los textos se guardan sin espacios sobrantes; el país en ISO 3166 alfa-2 (MX, US...).
/// </summary>
public sealed record Address
{
    public const int StreetMaxLength = 200;
    public const int CityMaxLength = 100;
    public const int StateMaxLength = 100;
    public const int PostalCodeMinLength = 3;
    public const int PostalCodeMaxLength = 10;

    private readonly string _street = string.Empty;
    private readonly string _city = string.Empty;
    private readonly string _state = string.Empty;
    private readonly string _postalCode = string.Empty;
    private readonly string _country = string.Empty;

    public string Street
    {
        get => _street;
        init => _street = Guard.RequiredText(value, nameof(Street), StreetMaxLength);
    }

    public string City
    {
        get => _city;
        init => _city = Guard.RequiredText(value, nameof(City), CityMaxLength);
    }

    public string State
    {
        get => _state;
        init => _state = Guard.RequiredText(value, nameof(State), StateMaxLength);
    }

    public string PostalCode
    {
        get => _postalCode;
        init
        {
            var text = Guard.RequiredText(value, nameof(PostalCode), PostalCodeMaxLength, PostalCodeMinLength);

            if (!text.All(c => char.IsAsciiLetterOrDigit(c) || c is ' ' or '-'))
                throw new ArgumentException("El código postal solo admite letras, dígitos, espacios y guiones.", nameof(PostalCode));

            _postalCode = text.ToUpperInvariant();
        }
    }

    public string Country
    {
        get => _country;
        init
        {
            var text = Guard.RequiredText(value, nameof(Country), maxLength: 2, minLength: 2);

            if (!text.All(char.IsAsciiLetter))
                throw new ArgumentException("El país debe ser un código ISO de 2 letras (ej. MX).", nameof(Country));

            _country = text.ToUpperInvariant();
        }
    }

    public Address(string street, string city, string state, string postalCode, string country)
    {
        Street = street;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }
}
