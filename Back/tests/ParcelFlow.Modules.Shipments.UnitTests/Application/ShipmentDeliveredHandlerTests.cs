using Microsoft.Extensions.Logging.Abstractions;
using ParcelFlow.Modules.Deliveries.IntegrationEvents;
using ParcelFlow.Modules.Shipments.Application.IntegrationEventHandlers;
using ParcelFlow.Modules.Shipments.Domain;

namespace ParcelFlow.Modules.Shipments.UnitTests.Application;

public class ShipmentDeliveredHandlerTests
{
    private readonly FakeShipmentRepository _repository = new();
    private readonly FakeShipmentsUnitOfWork _unitOfWork = new();

    private ShipmentDeliveredHandler Handler() => new(_repository, _unitOfWork, NullLogger<ShipmentDeliveredHandler>.Instance);

    private static ShipmentDeliveredIntegrationEvent DeliveredEvent(Shipment shipment) =>
        new(Guid.NewGuid(), shipment.Id, shipment.TrackingNumber.Value, "Luis Pérez", TestData.Now.AddDays(2));

    [Fact]
    public async Task Marks_the_shipment_as_delivered()
    {
        var shipment = TestData.Shipment();
        _repository.Add(shipment);

        await Handler().Handle(DeliveredEvent(shipment), CancellationToken.None);

        Assert.Equal(ShipmentStatus.Delivered, shipment.Status);
        Assert.Equal(TestData.Now.AddDays(2), shipment.DeliveredAt);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Receiving_the_same_event_twice_is_harmless()
    {
        var shipment = TestData.Shipment();
        _repository.Add(shipment);
        var e = DeliveredEvent(shipment);

        await Handler().Handle(e, CancellationToken.None);
        await Handler().Handle(e, CancellationToken.None);

        Assert.Equal(ShipmentStatus.Delivered, shipment.Status);
    }

    [Fact]
    public async Task A_cancelled_shipment_is_left_as_is_instead_of_failing_forever()
    {
        var shipment = TestData.Shipment();
        shipment.Cancel("Cliente se arrepintió", TestData.Now.AddHours(1));
        _repository.Add(shipment);

        await Handler().Handle(DeliveredEvent(shipment), CancellationToken.None);

        Assert.Equal(ShipmentStatus.Cancelled, shipment.Status);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Unknown_shipment_is_ignored()
    {
        await Handler().Handle(DeliveredEvent(TestData.Shipment()), CancellationToken.None);

        Assert.Equal(0, _unitOfWork.SaveCount);
    }
}
