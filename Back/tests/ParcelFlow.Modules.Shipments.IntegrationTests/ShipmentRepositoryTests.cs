using Microsoft.EntityFrameworkCore;
using ParcelFlow.Modules.Shipments.Application.Abstractions;
using ParcelFlow.Modules.Shipments.Domain;
using ParcelFlow.Modules.Shipments.Infrastructure.Persistence;

namespace ParcelFlow.Modules.Shipments.IntegrationTests;

public class ShipmentRepositoryTests(PostgresFixture fixture) : IClassFixture<PostgresFixture>
{
    private static readonly DateTime Now = new(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc);

    private static Shipment NewShipment(TrackingNumber? trackingNumber = null) =>
        Shipment.Create(
            trackingNumber ?? TrackingNumber.Generate(),
            new Contact("Ana López", "81 1234 5678", new Address("Av. Constitución 100", "Monterrey", "Nuevo León", "64000", "MX")),
            new Contact("Luis Pérez", "+52 33 9876 5432", new Address("Calle Juárez 5", "Guadalajara", "Jalisco", "44100", "MX")),
            new Weight(5.25m),
            new Dimensions(30m, 20m, 10m),
            Now);

    private async Task SaveAsync(Shipment shipment)
    {
        await using var db = fixture.CreateDbContext();
        new ShipmentRepository(db).Add(shipment);
        await ((IShipmentsUnitOfWork)db).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Saved_shipment_is_read_back_with_all_value_objects()
    {
        var original = NewShipment();
        await SaveAsync(original);

        await using var db = fixture.CreateDbContext();
        var loaded = await new ShipmentRepository(db).GetByIdAsync(original.Id, CancellationToken.None);

        Assert.NotNull(loaded);
        // Los value objects son records: Equal compara valor por valor (incluida la Address anidada).
        Assert.Equal(original.TrackingNumber, loaded.TrackingNumber);
        Assert.Equal(original.Sender, loaded.Sender);
        Assert.Equal(original.Recipient, loaded.Recipient);
        Assert.Equal(original.Weight, loaded.Weight);
        Assert.Equal(original.Dimensions, loaded.Dimensions);
        Assert.Equal(original.Cost, loaded.Cost);
        Assert.Equal(ShipmentStatus.Registered, loaded.Status);
        Assert.Equal(Now, loaded.CreatedAt);
    }

    [Fact]
    public async Task Finds_by_tracking_number()
    {
        var original = NewShipment();
        await SaveAsync(original);

        await using var db = fixture.CreateDbContext();
        var loaded = await new ShipmentRepository(db)
            .GetByTrackingNumberAsync(TrackingNumber.From(original.TrackingNumber.Value), CancellationToken.None);

        Assert.Equal(original.Id, loaded?.Id);
    }

    [Fact]
    public async Task Changes_made_through_the_domain_are_saved_without_an_Update_call()
    {
        var original = NewShipment();
        await SaveAsync(original);

        // Petición 1: cargar, cancelar y guardar (sin repository.Update: change tracking).
        await using (var db = fixture.CreateDbContext())
        {
            var shipment = await new ShipmentRepository(db).GetByIdAsync(original.Id, CancellationToken.None);
            shipment!.Cancel("Cliente se arrepintió", Now.AddHours(1));
            await ((IShipmentsUnitOfWork)db).SaveChangesAsync(CancellationToken.None);
        }

        // Petición 2: volver a leer desde cero.
        await using var readDb = fixture.CreateDbContext();
        var reloaded = await new ShipmentRepository(readDb).GetByIdAsync(original.Id, CancellationToken.None);

        Assert.Equal(ShipmentStatus.Cancelled, reloaded!.Status);
        Assert.Equal("Cliente se arrepintió", reloaded.CancellationReason);
    }

    [Fact]
    public async Task Duplicate_tracking_number_is_rejected_by_the_database()
    {
        var trackingNumber = TrackingNumber.Generate();
        await SaveAsync(NewShipment(trackingNumber));

        await Assert.ThrowsAsync<DbUpdateException>(() => SaveAsync(NewShipment(trackingNumber)));
    }

    [Fact]
    public async Task Table_lives_in_the_shipments_schema()
    {
        await using var db = fixture.CreateDbContext();

        var entity = db.Model.FindEntityType(typeof(Shipment))!;

        Assert.Equal(ShipmentsDbContext.Schema, entity.GetSchema());
    }
}
