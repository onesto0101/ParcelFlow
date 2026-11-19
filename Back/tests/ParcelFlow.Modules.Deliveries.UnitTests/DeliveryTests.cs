using ParcelFlow.BuildingBlocks.Domain;
using ParcelFlow.Modules.Deliveries.Domain;

namespace ParcelFlow.Modules.Deliveries.UnitTests;

public class DeliveryTests
{
    private static readonly DateTime Now = new(2026, 11, 19, 10, 0, 0, DateTimeKind.Utc);
    private static readonly CourierId Courier = new("REP-014");
    private static readonly FailureReason Absent = new(DeliveryFailureCode.RecipientAbsent);

    private static Delivery PendingDelivery() =>
        Delivery.CreatePending(Guid.NewGuid(), "PF23456789AB", Now);

    private static Delivery AssignedDelivery()
    {
        var delivery = PendingDelivery();
        delivery.AssignCourier(Courier, Now.AddHours(1));
        return delivery;
    }

    // --- Creación y asignación ---

    [Fact]
    public void CreatePending_StartsPendingWithoutAttempts()
    {
        var delivery = PendingDelivery();

        Assert.Equal(DeliveryStatus.Pending, delivery.Status);
        Assert.Null(delivery.CourierId);
        Assert.Empty(delivery.Attempts);
        Assert.Equal(Delivery.MaxAttempts, delivery.RemainingAttempts);
    }

    [Fact]
    public void AssignCourier_Pending_SetsAssignedAndRaisesEvent()
    {
        var delivery = AssignedDelivery();

        Assert.Equal(DeliveryStatus.Assigned, delivery.Status);
        Assert.Equal(Courier, delivery.CourierId);
        Assert.IsType<DeliveryAssignedDomainEvent>(Assert.Single(delivery.DomainEvents));
    }

    [Fact]
    public void AssignCourier_AlreadyAssigned_Reassigns()
    {
        var delivery = AssignedDelivery();
        var otherCourier = new CourierId("REP-020");

        delivery.AssignCourier(otherCourier, Now.AddHours(2));

        Assert.Equal(otherCourier, delivery.CourierId);
    }

    [Fact]
    public void AssignCourier_Delivered_ThrowsDomainException()
    {
        var delivery = AssignedDelivery();
        delivery.ConfirmDelivery(new DeliveryEvidence("María Gómez"), Now.AddHours(3));

        var exception = Assert.Throws<DomainException>(() => delivery.AssignCourier(Courier, Now.AddHours(4)));

        Assert.Equal("delivery.already_delivered", exception.Code);
    }

    // --- Confirmación ---

    [Fact]
    public void ConfirmDelivery_Assigned_RecordsSuccessfulAttempt()
    {
        var delivery = AssignedDelivery();
        var deliveredAt = Now.AddHours(3);

        delivery.ConfirmDelivery(new DeliveryEvidence("María Gómez"), deliveredAt);

        Assert.Equal(DeliveryStatus.Delivered, delivery.Status);
        Assert.Equal(deliveredAt, delivery.CompletedAt);

        var attempt = Assert.Single(delivery.Attempts);
        Assert.True(attempt.Succeeded);
        Assert.Equal("María Gómez", attempt.Evidence!.ReceivedBy);
        Assert.Equal(Courier, attempt.CourierId);
        Assert.Null(attempt.FailureReason);

        Assert.Contains(delivery.DomainEvents, e => e is ShipmentDeliveredDomainEvent);
    }

    [Fact]
    public void ConfirmDelivery_WithoutCourier_ThrowsDomainException()
    {
        var delivery = PendingDelivery();

        var exception = Assert.Throws<DomainException>(
            () => delivery.ConfirmDelivery(new DeliveryEvidence("María Gómez"), Now));

        Assert.Equal("delivery.not_assigned", exception.Code);
    }

    // --- Fallos e intentos ---

    [Fact]
    public void ReportFailure_FirstAttempt_ReturnsToPendingWithoutCourier()
    {
        var delivery = AssignedDelivery();

        delivery.ReportFailure(Absent, Now.AddHours(3));

        Assert.Equal(DeliveryStatus.Pending, delivery.Status);
        Assert.Null(delivery.CourierId);
        Assert.Equal(Delivery.MaxAttempts - 1, delivery.RemainingAttempts);

        var attempt = Assert.Single(delivery.Attempts);
        Assert.False(attempt.Succeeded);
        Assert.Equal(Absent, attempt.FailureReason);

        var failed = Assert.Single(delivery.DomainEvents.OfType<DeliveryAttemptFailedDomainEvent>());
        Assert.False(failed.IsFinal);
    }

    [Fact]
    public void ReportFailure_ThirdAttempt_MarksDeliveryAsFailed()
    {
        var delivery = PendingDelivery();

        for (var i = 1; i <= Delivery.MaxAttempts; i++)
        {
            delivery.AssignCourier(Courier, Now.AddDays(i));
            delivery.ReportFailure(Absent, Now.AddDays(i).AddHours(4));
        }

        Assert.Equal(DeliveryStatus.Failed, delivery.Status);
        Assert.Equal(0, delivery.RemainingAttempts);
        Assert.Equal([1, 2, 3], delivery.Attempts.Select(a => a.AttemptNumber));
        Assert.True(delivery.DomainEvents.OfType<DeliveryAttemptFailedDomainEvent>().Last().IsFinal);
    }

    [Fact]
    public void AssignCourier_AfterMaxAttempts_ThrowsDomainException()
    {
        var delivery = PendingDelivery();
        for (var i = 1; i <= Delivery.MaxAttempts; i++)
        {
            delivery.AssignCourier(Courier, Now.AddDays(i));
            delivery.ReportFailure(Absent, Now.AddDays(i).AddHours(4));
        }

        var exception = Assert.Throws<DomainException>(() => delivery.AssignCourier(Courier, Now.AddDays(10)));

        Assert.Equal("delivery.max_attempts_reached", exception.Code);
    }

    [Fact]
    public void ReportFailure_WithoutCourier_ThrowsDomainException()
    {
        var delivery = PendingDelivery();

        Assert.Throws<DomainException>(() => delivery.ReportFailure(Absent, Now));
    }

    [Fact]
    public void SuccessAfterFailure_IsSecondAttempt()
    {
        var delivery = AssignedDelivery();
        delivery.ReportFailure(Absent, Now.AddHours(3));
        delivery.AssignCourier(Courier, Now.AddDays(1));

        delivery.ConfirmDelivery(new DeliveryEvidence("María Gómez"), Now.AddDays(1).AddHours(2));

        Assert.Equal(DeliveryStatus.Delivered, delivery.Status);
        Assert.Equal(2, delivery.Attempts.Count);
        Assert.True(delivery.Attempts[1].Succeeded);
    }

    // --- Cancelación ---

    [Fact]
    public void Cancel_Assigned_CancelsAndReleasesCourier()
    {
        var delivery = AssignedDelivery();

        delivery.Cancel(Now.AddHours(2));

        Assert.Equal(DeliveryStatus.Cancelled, delivery.Status);
        Assert.Null(delivery.CourierId);
    }

    [Fact]
    public void Cancel_Twice_IsIdempotent()
    {
        var delivery = PendingDelivery();
        delivery.Cancel(Now.AddHours(1));
        delivery.ClearDomainEvents();

        delivery.Cancel(Now.AddHours(2));

        Assert.Empty(delivery.DomainEvents);
    }

    [Fact]
    public void Cancel_Delivered_ThrowsDomainException()
    {
        var delivery = AssignedDelivery();
        delivery.ConfirmDelivery(new DeliveryEvidence("María Gómez"), Now.AddHours(3));

        Assert.Throws<DomainException>(() => delivery.Cancel(Now.AddHours(4)));
    }
}
