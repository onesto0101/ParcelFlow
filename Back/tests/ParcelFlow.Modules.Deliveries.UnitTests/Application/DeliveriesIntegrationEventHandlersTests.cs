using Microsoft.Extensions.Logging.Abstractions;
using ParcelFlow.Modules.Deliveries.Application.IntegrationEventHandlers;
using ParcelFlow.Modules.Deliveries.Domain;
using ParcelFlow.Modules.Shipments.IntegrationEvents;
using ParcelFlow.Modules.Tracking.IntegrationEvents;

namespace ParcelFlow.Modules.Deliveries.UnitTests.Application;

public class DeliveriesIntegrationEventHandlersTests
{
    private static readonly DateTime Now = new(2026, 11, 23, 10, 0, 0, DateTimeKind.Utc);

    private readonly FakeDeliveryRepository _repository = new();
    private readonly FakeDeliveriesUnitOfWork _unitOfWork = new();

    private DeliveriesIntegrationEventHandlers Handlers() =>
        new(_repository, _unitOfWork, NullLogger<DeliveriesIntegrationEventHandlers>.Instance);

    private static ShipmentReadyForDeliveryIntegrationEvent Ready(Guid shipmentId) =>
        new(shipmentId, "PF23456789AB", "CEDIS GDL", Now);

    [Fact]
    public async Task Ready_for_delivery_creates_a_pending_delivery()
    {
        var shipmentId = Guid.NewGuid();

        await Handlers().Handle(Ready(shipmentId), CancellationToken.None);

        var delivery = Assert.Single(_repository.Items);
        Assert.Equal(shipmentId, delivery.ShipmentId);
        Assert.Equal(DeliveryStatus.Pending, delivery.Status);
    }

    [Fact]
    public async Task Arriving_at_a_second_center_does_not_create_a_second_delivery()
    {
        var shipmentId = Guid.NewGuid();

        await Handlers().Handle(Ready(shipmentId), CancellationToken.None);
        await Handlers().Handle(Ready(shipmentId), CancellationToken.None);

        Assert.Single(_repository.Items);
    }

    [Fact]
    public async Task Shipment_cancelled_cancels_the_delivery()
    {
        var shipmentId = Guid.NewGuid();
        await Handlers().Handle(Ready(shipmentId), CancellationToken.None);

        await Handlers().Handle(new ShipmentCancelledIntegrationEvent(shipmentId, "PF23456789AB", "Cliente", Now.AddHours(1)), CancellationToken.None);

        Assert.Equal(DeliveryStatus.Cancelled, _repository.Items.Single().Status);
    }

    [Fact]
    public async Task Shipment_cancelled_before_reaching_a_center_does_nothing()
    {
        await Handlers().Handle(new ShipmentCancelledIntegrationEvent(Guid.NewGuid(), "PF23456789AB", "Cliente", Now), CancellationToken.None);

        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Shipment_cancelled_after_delivery_keeps_it_delivered()
    {
        var delivery = Delivery.CreatePending(Guid.NewGuid(), "PF23456789AB", Now);
        delivery.AssignCourier(new CourierId("REP-014"), Now.AddHours(1));
        delivery.ConfirmDelivery(new DeliveryEvidence("Luis Pérez"), Now.AddHours(2));
        _repository.Add(delivery);

        await Handlers().Handle(new ShipmentCancelledIntegrationEvent(delivery.ShipmentId, "PF23456789AB", "Tarde", Now.AddHours(3)), CancellationToken.None);

        Assert.Equal(DeliveryStatus.Delivered, delivery.Status);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }
}
