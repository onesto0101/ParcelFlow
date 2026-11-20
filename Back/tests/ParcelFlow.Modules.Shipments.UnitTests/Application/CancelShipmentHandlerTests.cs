using Microsoft.Extensions.Time.Testing;
using ParcelFlow.BuildingBlocks.Application.Exceptions;
using ParcelFlow.BuildingBlocks.Domain;
using ParcelFlow.Modules.Shipments.Application.CancelShipment;
using ParcelFlow.Modules.Shipments.Domain;

namespace ParcelFlow.Modules.Shipments.UnitTests.Application;

public class CancelShipmentHandlerTests
{
    private readonly FakeShipmentRepository _repository = new();
    private readonly FakeShipmentsUnitOfWork _unitOfWork = new();
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(TestData.Now));

    private CancelShipmentHandler CreateHandler() => new(_repository, _unitOfWork, _time);

    private Shipment ExistingShipment()
    {
        var shipment = TestData.Shipment();
        _repository.Add(shipment);
        return shipment;
    }

    [Fact]
    public async Task Cancels_and_saves()
    {
        var shipment = ExistingShipment();
        _time.Advance(TimeSpan.FromHours(2));

        await CreateHandler().Handle(new CancelShipmentCommand(shipment.Id, "Cliente se arrepintió"), CancellationToken.None);

        Assert.Equal(ShipmentStatus.Cancelled, shipment.Status);
        Assert.Equal(TestData.Now.AddHours(2), shipment.CancelledAt);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Unknown_id_throws_NotFound()
    {
        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateHandler().Handle(new CancelShipmentCommand(Guid.NewGuid(), "x"), CancellationToken.None));

        Assert.Equal("shipment.not_found", ex.Code);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Domain_rule_violation_bubbles_up_and_nothing_is_saved()
    {
        var shipment = ExistingShipment();
        shipment.Cancel("primera vez", TestData.Now);

        var ex = await Assert.ThrowsAsync<DomainException>(
            () => CreateHandler().Handle(new CancelShipmentCommand(shipment.Id, "segunda vez"), CancellationToken.None));

        Assert.Equal("shipment.already_cancelled", ex.Code);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }
}
