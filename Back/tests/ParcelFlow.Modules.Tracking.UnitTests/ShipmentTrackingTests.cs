using ParcelFlow.BuildingBlocks.Domain;
using ParcelFlow.Modules.Tracking.Domain;

namespace ParcelFlow.Modules.Tracking.UnitTests;

public class ShipmentTrackingTests
{
    private static readonly DateTime Now = new(2026, 11, 19, 10, 0, 0, DateTimeKind.Utc);
    private static readonly Guid ShipmentId = Guid.NewGuid();

    private static ShipmentTracking StartTracking() =>
        ShipmentTracking.Start(ShipmentId, "pf23456789ab", new Location("Monterrey"), Now);

    private static void Move(ShipmentTracking tracking, TrackingStatus status, int hoursLater, string location = "CEDIS Monterrey") =>
        tracking.RegisterEvent(status, new Location(location), new TrackingDescription($"Movimiento {status}"), Now.AddHours(hoursLater));

    // --- Start ---

    [Fact]
    public void Start_CreatesFirstEventWithCreatedStatus()
    {
        var tracking = StartTracking();

        Assert.Equal(TrackingStatus.Created, tracking.CurrentStatus);
        Assert.Equal("PF23456789AB", tracking.TrackingNumber);
        Assert.Equal("Monterrey", tracking.CurrentLocation.Name);

        var firstEvent = Assert.Single(tracking.Events);
        Assert.Equal(TrackingStatus.Created, firstEvent.Status);
    }

    [Fact]
    public void Start_EmptyShipmentId_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            ShipmentTracking.Start(Guid.Empty, "PF23456789AB", new Location("Monterrey"), Now));
    }

    // --- RegisterEvent ---

    [Fact]
    public void RegisterEvent_ValidTransition_UpdatesCurrentStateAndKeepsHistory()
    {
        var tracking = StartTracking();

        Move(tracking, TrackingStatus.PickedUp, 1, "Sucursal Centro");
        Move(tracking, TrackingStatus.InTransit, 2, "Carretera 85");

        Assert.Equal(TrackingStatus.InTransit, tracking.CurrentStatus);
        Assert.Equal("Carretera 85", tracking.CurrentLocation.Name);
        Assert.Equal(Now.AddHours(2), tracking.LastUpdatedAt);
        Assert.Equal(
            [TrackingStatus.Created, TrackingStatus.PickedUp, TrackingStatus.InTransit],
            tracking.Events.Select(e => e.Status));
    }

    [Fact]
    public void RegisterEvent_InvalidTransition_ThrowsDomainException()
    {
        var tracking = StartTracking();

        var exception = Assert.Throws<DomainException>(() => Move(tracking, TrackingStatus.Delivered, 1));

        Assert.Equal("tracking.invalid_transition", exception.Code);
        Assert.Single(tracking.Events); // el historial no cambió
    }

    [Fact]
    public void RegisterEvent_DeliveredWithoutOutForDelivery_ThrowsDomainException()
    {
        var tracking = StartTracking();
        Move(tracking, TrackingStatus.PickedUp, 1);
        Move(tracking, TrackingStatus.InTransit, 2);
        Move(tracking, TrackingStatus.AtDistributionCenter, 3);

        Assert.Throws<DomainException>(() => Move(tracking, TrackingStatus.Delivered, 4));
    }

    [Fact]
    public void RegisterEvent_OlderThanLastEvent_ThrowsDomainException()
    {
        var tracking = StartTracking();
        Move(tracking, TrackingStatus.PickedUp, 5);

        var exception = Assert.Throws<DomainException>(() => Move(tracking, TrackingStatus.InTransit, 4));

        Assert.Equal("tracking.event_out_of_order", exception.Code);
    }

    [Fact]
    public void RegisterEvent_RaisesStatusChangedEvent()
    {
        var tracking = StartTracking();

        Move(tracking, TrackingStatus.PickedUp, 1);

        var domainEvent = Assert.IsType<ShipmentStatusChangedDomainEvent>(Assert.Single(tracking.DomainEvents));
        Assert.Equal(TrackingStatus.Created, domainEvent.PreviousStatus);
        Assert.Equal(TrackingStatus.PickedUp, domainEvent.NewStatus);
        Assert.Equal(ShipmentId, domainEvent.ShipmentId);
    }

    [Fact]
    public void RegisterEvent_AtDistributionCenter_AlsoRaisesArrivedAtCenterEvent()
    {
        var tracking = StartTracking();
        Move(tracking, TrackingStatus.PickedUp, 1);
        Move(tracking, TrackingStatus.InTransit, 2);
        tracking.ClearDomainEvents();

        Move(tracking, TrackingStatus.AtDistributionCenter, 3, "CEDIS Guadalajara");

        Assert.Contains(tracking.DomainEvents, e => e is ShipmentStatusChangedDomainEvent);
        var arrived = Assert.Single(tracking.DomainEvents.OfType<ShipmentArrivedAtCenterDomainEvent>());
        Assert.Equal("CEDIS Guadalajara", arrived.Location);
    }

    [Fact]
    public void Events_CannotBeModifiedFromOutside()
    {
        var tracking = StartTracking();

        // La colección expuesta es de solo lectura: intentar castearla a List no da acceso a la interna.
        Assert.False(tracking.Events is List<TrackingEvent>);
    }
}
