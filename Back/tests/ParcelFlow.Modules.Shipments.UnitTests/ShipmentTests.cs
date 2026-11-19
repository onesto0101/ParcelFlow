using ParcelFlow.BuildingBlocks.Domain;
using ParcelFlow.Modules.Shipments.Domain;

namespace ParcelFlow.Modules.Shipments.UnitTests;

public class ShipmentTests
{
    // --- Create ---

    [Fact]
    public void Create_SetsRegisteredStatusAndCalculatesCost()
    {
        var shipment = TestData.Shipment();

        Assert.NotEqual(Guid.Empty, shipment.Id);
        Assert.Equal(ShipmentStatus.Registered, shipment.Status);
        Assert.Equal(TestData.Now, shipment.CreatedAt);
        Assert.Equal(ShippingCostCalculator.Calculate(shipment.Weight, shipment.Dimensions), shipment.Cost);
    }

    [Fact]
    public void Create_RaisesShipmentCreatedEvent()
    {
        var shipment = TestData.Shipment();

        var domainEvent = Assert.IsType<ShipmentCreatedDomainEvent>(Assert.Single(shipment.DomainEvents));
        Assert.Equal(shipment.Id, domainEvent.ShipmentId);
        Assert.Equal(shipment.TrackingNumber.Value, domainEvent.TrackingNumber);
        Assert.Equal("Monterrey", domainEvent.OriginCity);
        Assert.Equal("Guadalajara", domainEvent.DestinationCity);
    }

    [Fact]
    public void Create_WithoutRecipient_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => Shipment.Create(
            TrackingNumber.Generate(), TestData.Contact(), null!,
            new Weight(5m), new Dimensions(30m, 20m, 10m), TestData.Now));
    }

    // --- Cancel ---

    [Fact]
    public void Cancel_RegisteredShipment_KeepsHistoryAndRaisesEvent()
    {
        var shipment = TestData.Shipment();
        shipment.ClearDomainEvents();
        var cancelledAt = TestData.Now.AddHours(1);

        shipment.Cancel("  El cliente se arrepintió  ", cancelledAt);

        Assert.Equal(ShipmentStatus.Cancelled, shipment.Status);
        Assert.Equal(cancelledAt, shipment.CancelledAt);
        Assert.Equal("El cliente se arrepintió", shipment.CancellationReason);
        Assert.IsType<ShipmentCancelledDomainEvent>(Assert.Single(shipment.DomainEvents));
    }

    [Fact]
    public void Cancel_WithoutReason_ThrowsException()
    {
        var shipment = TestData.Shipment();

        Assert.Throws<ArgumentException>(() => shipment.Cancel(" ", TestData.Now));
    }

    [Fact]
    public void Cancel_DeliveredShipment_ThrowsDomainException()
    {
        var shipment = TestData.Shipment();
        shipment.MarkAsDelivered(TestData.Now.AddDays(2));

        var exception = Assert.Throws<DomainException>(() => shipment.Cancel("Tarde", TestData.Now.AddDays(3)));

        Assert.Equal("shipment.already_delivered", exception.Code);
    }

    [Fact]
    public void Cancel_AlreadyCancelled_ThrowsDomainException()
    {
        var shipment = TestData.Shipment();
        shipment.Cancel("Primera vez", TestData.Now);

        var exception = Assert.Throws<DomainException>(() => shipment.Cancel("Segunda vez", TestData.Now));

        Assert.Equal("shipment.already_cancelled", exception.Code);
    }

    // --- MarkAsDelivered ---

    [Fact]
    public void MarkAsDelivered_RegisteredShipment_SetsDelivered()
    {
        var shipment = TestData.Shipment();
        var deliveredAt = TestData.Now.AddDays(2);

        shipment.MarkAsDelivered(deliveredAt);

        Assert.Equal(ShipmentStatus.Delivered, shipment.Status);
        Assert.Equal(deliveredAt, shipment.DeliveredAt);
    }

    [Fact]
    public void MarkAsDelivered_Twice_IsIdempotent()
    {
        var shipment = TestData.Shipment();
        var firstTime = TestData.Now.AddDays(2);

        shipment.MarkAsDelivered(firstTime);
        shipment.MarkAsDelivered(firstTime.AddHours(1));

        Assert.Equal(firstTime, shipment.DeliveredAt);
    }

    [Fact]
    public void MarkAsDelivered_CancelledShipment_ThrowsDomainException()
    {
        var shipment = TestData.Shipment();
        shipment.Cancel("Cancelado", TestData.Now);

        var exception = Assert.Throws<DomainException>(() => shipment.MarkAsDelivered(TestData.Now.AddDays(1)));

        Assert.Equal("shipment.cancelled", exception.Code);
    }

    // --- Entity equality ---

    [Fact]
    public void TwoShipments_WithSameData_AreNotEqual_BecauseIdentityDiffers()
    {
        // Contraste con los value objects: mismos datos, pero son DOS envíos distintos.
        Assert.NotEqual(TestData.Shipment(), TestData.Shipment());
    }
}
