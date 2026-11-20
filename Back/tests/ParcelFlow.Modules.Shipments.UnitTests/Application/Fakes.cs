using ParcelFlow.Modules.Shipments.Application.Abstractions;
using ParcelFlow.Modules.Shipments.Application.CreateShipment;
using ParcelFlow.Modules.Shipments.Domain;

namespace ParcelFlow.Modules.Shipments.UnitTests.Application;

/// <summary>
/// Repositorio falso en memoria: una simple lista. Permite probar los handlers
/// sin base de datos (rápido y sin Docker). Se cambiará por EF Core en Infrastructure.
/// </summary>
internal sealed class FakeShipmentRepository : IShipmentRepository
{
    public List<Shipment> Shipments { get; } = [];

    public Task<Shipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Shipments.SingleOrDefault(s => s.Id == id));

    public Task<Shipment?> GetByTrackingNumberAsync(TrackingNumber trackingNumber, CancellationToken cancellationToken) =>
        Task.FromResult(Shipments.SingleOrDefault(s => s.TrackingNumber == trackingNumber));

    public void Add(Shipment shipment) => Shipments.Add(shipment);
}

/// <summary>
/// Unit of work falso: solo cuenta cuántas veces se guardó. Así un test puede comprobar
/// "se guardó exactamente una vez" o "no se guardó nada porque falló la validación".
/// </summary>
internal sealed class FakeShipmentsUnitOfWork : IShipmentsUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}

internal static class CommandData
{
    public static ContactInput Contact(string name = "Ana López", string city = "Monterrey") =>
        new(name, "81 1234 5678", new AddressInput("Av. Constitución 100", city, "Nuevo León", "64000", "MX"));

    public static CreateShipmentCommand CreateShipment(decimal weightKg = 5m) =>
        new(Contact("Ana López", "Monterrey"), Contact("Luis Pérez", "Guadalajara"), weightKg, 30m, 20m, 10m);
}
