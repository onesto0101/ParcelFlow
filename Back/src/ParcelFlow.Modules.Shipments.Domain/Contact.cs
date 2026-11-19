using ParcelFlow.BuildingBlocks.Domain;

namespace ParcelFlow.Modules.Shipments.Domain;

/// <summary>
/// Datos de una persona que envía o recibe un paquete (nombre, teléfono y dirección).
///
/// Decisión de diseño: el documento de diseño propone Sender y Recipient como entidades,
/// pero dentro de un envío no tienen identidad ni ciclo de vida propio: son una "foto"
/// de los datos en el momento de registrar el envío. Si mañana el cliente cambia de
/// teléfono, los envíos antiguos deben conservar el teléfono que tenían. Por eso son un
/// value object, y uno solo (Contact) sirve para los dos roles.
/// </summary>
public sealed record Contact
{
    public const int NameMinLength = 2;
    public const int NameMaxLength = 150;
    public const int PhoneMinDigits = 7;
    public const int PhoneMaxDigits = 15; // máximo del estándar internacional E.164

    private readonly string _name = string.Empty;
    private readonly string _phone = string.Empty;
    private readonly Address _address = null!;

    public string Name
    {
        get => _name;
        init => _name = Guard.RequiredText(value, nameof(Name), NameMaxLength, NameMinLength);
    }

    /// <summary>
    /// Teléfono normalizado: solo dígitos, con "+" opcional al inicio.
    /// "(81) 1234-5678" se guarda como "8112345678".
    /// </summary>
    public string Phone
    {
        get => _phone;
        init => _phone = NormalizePhone(value);
    }

    public Address Address
    {
        get => _address;
        init => _address = value ?? throw new ArgumentNullException(nameof(Address));
    }

    public Contact(string name, string phone, Address address)
    {
        Name = name;
        Phone = phone;
        Address = address;
    }

    private static string NormalizePhone(string? value)
    {
        var text = Guard.RequiredText(value, nameof(Phone), maxLength: 30);

        var hasPlus = text.StartsWith('+');
        var withoutSeparators = new string(text.Where(c => c is not (' ' or '-' or '(' or ')' or '+')).ToArray());

        var isValid =
            withoutSeparators.All(char.IsAsciiDigit) &&
            withoutSeparators.Length is >= PhoneMinDigits and <= PhoneMaxDigits &&
            text.LastIndexOf('+') <= 0; // el "+" solo puede ir al principio

        if (!isValid)
            throw new ArgumentException(
                $"Teléfono inválido. Debe tener entre {PhoneMinDigits} y {PhoneMaxDigits} dígitos.",
                nameof(Phone));

        return hasPlus ? "+" + withoutSeparators : withoutSeparators;
    }
}
