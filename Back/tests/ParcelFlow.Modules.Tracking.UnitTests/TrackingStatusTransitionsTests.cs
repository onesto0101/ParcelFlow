using ParcelFlow.Modules.Tracking.Domain;

namespace ParcelFlow.Modules.Tracking.UnitTests;

public class TrackingStatusTransitionsTests
{
    [Theory]
    [InlineData(TrackingStatus.Created, TrackingStatus.PickedUp)]
    [InlineData(TrackingStatus.PickedUp, TrackingStatus.InTransit)]
    [InlineData(TrackingStatus.InTransit, TrackingStatus.AtDistributionCenter)]
    [InlineData(TrackingStatus.AtDistributionCenter, TrackingStatus.InTransit)]
    [InlineData(TrackingStatus.AtDistributionCenter, TrackingStatus.OutForDelivery)]
    [InlineData(TrackingStatus.OutForDelivery, TrackingStatus.Delivered)]
    [InlineData(TrackingStatus.OutForDelivery, TrackingStatus.DeliveryFailed)]
    [InlineData(TrackingStatus.DeliveryFailed, TrackingStatus.OutForDelivery)]
    [InlineData(TrackingStatus.Created, TrackingStatus.Cancelled)]
    [InlineData(TrackingStatus.OutForDelivery, TrackingStatus.Cancelled)]
    public void CanTransition_AllowedTransitions_ReturnsTrue(TrackingStatus from, TrackingStatus to)
    {
        Assert.True(TrackingStatusTransitions.CanTransition(from, to));
    }

    [Theory]
    [InlineData(TrackingStatus.Created, TrackingStatus.Delivered)]               // saltarse todo el recorrido
    [InlineData(TrackingStatus.AtDistributionCenter, TrackingStatus.Delivered)]  // entregar sin salir a reparto
    [InlineData(TrackingStatus.InTransit, TrackingStatus.OutForDelivery)]        // reparto sin pasar por un centro
    [InlineData(TrackingStatus.Delivered, TrackingStatus.Cancelled)]             // final
    [InlineData(TrackingStatus.Cancelled, TrackingStatus.PickedUp)]              // final
    [InlineData(TrackingStatus.PickedUp, TrackingStatus.PickedUp)]               // al mismo estado
    public void CanTransition_ForbiddenTransitions_ReturnsFalse(TrackingStatus from, TrackingStatus to)
    {
        Assert.False(TrackingStatusTransitions.CanTransition(from, to));
    }

    [Theory]
    [InlineData(TrackingStatus.Delivered, true)]
    [InlineData(TrackingStatus.Cancelled, true)]
    [InlineData(TrackingStatus.DeliveryFailed, false)]
    [InlineData(TrackingStatus.Created, false)]
    public void IsFinal_ReturnsExpected(TrackingStatus status, bool expected)
    {
        Assert.Equal(expected, TrackingStatusTransitions.IsFinal(status));
    }

    [Fact]
    public void EveryStatus_HasTransitionsDefined()
    {
        // Si alguien añade un estado nuevo al enum y olvida la tabla, este test lo avisa.
        foreach (var status in Enum.GetValues<TrackingStatus>())
        {
            var isFinal = TrackingStatusTransitions.IsFinal(status);
            var isKnownFinal = status is TrackingStatus.Delivered or TrackingStatus.Cancelled;

            Assert.Equal(isKnownFinal, isFinal);
        }
    }
}
