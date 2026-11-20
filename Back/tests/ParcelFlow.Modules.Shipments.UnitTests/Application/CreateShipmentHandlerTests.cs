using Microsoft.Extensions.Time.Testing;
using ParcelFlow.Modules.Shipments.Application.CreateShipment;
using ParcelFlow.Modules.Shipments.Domain;

namespace ParcelFlow.Modules.Shipments.UnitTests.Application;

public class CreateShipmentHandlerTests
{
    private readonly FakeShipmentRepository _repository = new();
    private readonly FakeShipmentsUnitOfWork _unitOfWork = new();
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(TestData.Now));

    private CreateShipmentHandler CreateHandler() => new(_repository, _unitOfWork, _time);

    [Fact]
    public async Task Creates_and_saves_the_shipment_once()
    {
        var result = await CreateHandler().Handle(CommandData.CreateShipment(), CancellationToken.None);

        var saved = Assert.Single(_repository.Shipments);
        Assert.Equal(saved.Id, result.ShipmentId);
        Assert.Equal(saved.TrackingNumber.Value, result.TrackingNumber);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Uses_the_current_time_from_TimeProvider()
    {
        await CreateHandler().Handle(CommandData.CreateShipment(), CancellationToken.None);

        Assert.Equal(TestData.Now, _repository.Shipments[0].CreatedAt);
    }

    [Fact]
    public async Task Returns_the_cost_calculated_by_the_domain()
    {
        // 5 kg real vs 30x20x10/5000 = 1.2 kg volumétrico → se cobra 5 kg → 50 + 15*5 = 125
        var result = await CreateHandler().Handle(CommandData.CreateShipment(weightKg: 5m), CancellationToken.None);

        Assert.Equal(125m, result.Cost);
        Assert.Equal("MXN", result.Currency);
    }

    [Fact]
    public async Task Invalid_data_throws_and_nothing_is_saved()
    {
        var command = CommandData.CreateShipment(weightKg: -1m);

        await Assert.ThrowsAnyAsync<ArgumentException>(
            () => CreateHandler().Handle(command, CancellationToken.None));

        Assert.Empty(_repository.Shipments);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Raises_ShipmentCreated_domain_event()
    {
        await CreateHandler().Handle(CommandData.CreateShipment(), CancellationToken.None);

        Assert.IsType<ShipmentCreatedDomainEvent>(Assert.Single(_repository.Shipments[0].DomainEvents));
    }
}
