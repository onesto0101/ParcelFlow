using Microsoft.Extensions.Logging.Abstractions;
using ParcelFlow.Modules.Deliveries.IntegrationEvents;
using ParcelFlow.Modules.Shipments.IntegrationEvents;
using ParcelFlow.Modules.Tracking.Application.IntegrationEventHandlers;
using ParcelFlow.Modules.Tracking.Domain;

namespace ParcelFlow.Modules.Tracking.UnitTests.Application;

public class TrackingIntegrationEventHandlersTests
{
    private static readonly DateTime Now = TrackingData.Now;

    private readonly FakeTrackingRepository _repository = new();
    private readonly FakeTrackingUnitOfWork _unitOfWork = new();

    private TrackingIntegrationEventHandlers Handlers() =>
        new(_repository, _unitOfWork, NullLogger<TrackingIntegrationEventHandlers>.Instance);

    /// <summary>Lleva el seguimiento hasta "en centro de distribución", listo para reparto.</summary>
    private ShipmentTracking TrackingAtCenter()
    {
        var tracking = TrackingData.Started();
        tracking.RegisterEvent(TrackingStatus.PickedUp, new Location("Monterrey"), new TrackingDescription("Recolectado"), Now.AddHours(1));
        tracking.RegisterEvent(TrackingStatus.InTransit, new Location("Carretera"), new TrackingDescription("En camino"), Now.AddHours(2));
        tracking.RegisterEvent(TrackingStatus.AtDistributionCenter, new Location("CEDIS GDL"), new TrackingDescription("Llegó"), Now.AddHours(3));
        _repository.Add(tracking);
        return tracking;
    }

    [Fact]
    public async Task ShipmentCreated_starts_tracking_at_the_origin_city()
    {
        var shipmentId = Guid.NewGuid();

        await Handlers().Handle(
            new ShipmentCreatedIntegrationEvent(shipmentId, "PF23456789AB", "Monterrey", "Guadalajara", Now),
            CancellationToken.None);

        var tracking = Assert.Single(_repository.Items);
        Assert.Equal(shipmentId, tracking.ShipmentId);
        Assert.Equal(TrackingStatus.Created, tracking.CurrentStatus);
        Assert.Equal("Monterrey", tracking.CurrentLocation.Name);
    }

    [Fact]
    public async Task ShipmentCreated_twice_creates_a_single_tracking()
    {
        var e = new ShipmentCreatedIntegrationEvent(Guid.NewGuid(), "PF23456789AB", "Monterrey", "Guadalajara", Now);

        await Handlers().Handle(e, CancellationToken.None);
        await Handlers().Handle(e, CancellationToken.None);

        Assert.Single(_repository.Items);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task DeliveryAssigned_puts_the_package_out_for_delivery_at_its_current_location()
    {
        var tracking = TrackingAtCenter();

        await Handlers().Handle(
            new DeliveryAssignedIntegrationEvent(Guid.NewGuid(), tracking.ShipmentId, tracking.TrackingNumber, "REP-014", Now.AddHours(4)),
            CancellationToken.None);

        Assert.Equal(TrackingStatus.OutForDelivery, tracking.CurrentStatus);
        Assert.Equal("CEDIS GDL", tracking.CurrentLocation.Name);
        Assert.Contains("REP-014", tracking.Events[^1].Description.Value);
    }

    [Fact]
    public async Task Repeated_event_is_ignored_without_error()
    {
        var tracking = TrackingAtCenter();
        var assigned = new DeliveryAssignedIntegrationEvent(Guid.NewGuid(), tracking.ShipmentId, tracking.TrackingNumber, "REP-014", Now.AddHours(4));

        await Handlers().Handle(assigned, CancellationToken.None);
        await Handlers().Handle(assigned, CancellationToken.None); // OutForDelivery → OutForDelivery no existe

        Assert.Equal(5, tracking.Events.Count); // Created, PickedUp, InTransit, AtCenter, OutForDelivery
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Failed_attempt_then_delivered()
    {
        var tracking = TrackingAtCenter();
        var shipmentId = tracking.ShipmentId;
        var tn = tracking.TrackingNumber;

        await Handlers().Handle(new DeliveryAssignedIntegrationEvent(Guid.NewGuid(), shipmentId, tn, "REP-014", Now.AddHours(4)), CancellationToken.None);
        await Handlers().Handle(new DeliveryAttemptFailedIntegrationEvent(Guid.NewGuid(), shipmentId, tn, 1, "RecipientAbsent", false, Now.AddHours(5)), CancellationToken.None);

        Assert.Equal(TrackingStatus.DeliveryFailed, tracking.CurrentStatus);
        Assert.Contains("destinatario ausente", tracking.Events[^1].Description.Value);

        await Handlers().Handle(new DeliveryAssignedIntegrationEvent(Guid.NewGuid(), shipmentId, tn, "REP-020", Now.AddDays(1)), CancellationToken.None);
        await Handlers().Handle(new ShipmentDeliveredIntegrationEvent(Guid.NewGuid(), shipmentId, tn, "Luis Pérez", Now.AddDays(1).AddHours(1)), CancellationToken.None);

        Assert.Equal(TrackingStatus.Delivered, tracking.CurrentStatus);
    }

    [Fact]
    public async Task Event_older_than_the_last_movement_is_registered_with_the_last_movement_time()
    {
        var tracking = TrackingAtCenter(); // último movimiento: Now + 3 h

        await Handlers().Handle(
            new ShipmentCancelledIntegrationEvent(tracking.ShipmentId, tracking.TrackingNumber, "Cliente", Now.AddHours(1)),
            CancellationToken.None);

        Assert.Equal(TrackingStatus.Cancelled, tracking.CurrentStatus);
        Assert.Equal(Now.AddHours(3), tracking.LastUpdatedAt);
    }

    [Fact]
    public async Task Event_for_an_unknown_shipment_is_ignored()
    {
        await Handlers().Handle(
            new ShipmentCancelledIntegrationEvent(Guid.NewGuid(), "PF23456789AB", "Cliente", Now),
            CancellationToken.None);

        Assert.Equal(0, _unitOfWork.SaveCount);
    }
}
