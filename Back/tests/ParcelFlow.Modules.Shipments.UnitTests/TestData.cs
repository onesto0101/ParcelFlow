using ParcelFlow.Modules.Shipments.Domain;

namespace ParcelFlow.Modules.Shipments.UnitTests;

/// <summary>
/// Objetos válidos listos para usar en los tests ("Object Mother").
/// Así cada test solo escribe lo que le importa y no repite 15 líneas de datos.
/// </summary>
internal static class TestData
{
    public static readonly DateTime Now = new(2026, 11, 19, 10, 0, 0, DateTimeKind.Utc);

    public static Address Address(string city = "Monterrey") =>
        new("Av. Constitución 100", city, "Nuevo León", "64000", "MX");

    public static Contact Contact(string name = "Ana López", string city = "Monterrey") =>
        new(name, "81 1234 5678", Address(city));

    public static Shipment Shipment() =>
        Domain.Shipment.Create(
            TrackingNumber.Generate(),
            Contact("Ana López", "Monterrey"),
            Contact("Luis Pérez", "Guadalajara"),
            new Weight(5m),
            new Dimensions(30m, 20m, 10m),
            Now);
}
