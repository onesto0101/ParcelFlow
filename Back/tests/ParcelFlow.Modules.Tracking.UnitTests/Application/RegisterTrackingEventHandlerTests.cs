using Microsoft.Extensions.Time.Testing;
using ParcelFlow.BuildingBlocks.Application.Exceptions;
using ParcelFlow.BuildingBlocks.Domain;
using ParcelFlow.Modules.Tracking.Application.RegisterTrackingEvent;
using ParcelFlow.Modules.Tracking.Domain;

namespace ParcelFlow.Modules.Tracking.UnitTests.Application;

public class RegisterTrackingEventHandlerTests
{
    private readonly FakeTrackingRepository _repository = new();
    private readonly FakeTrackingUnitOfWork _unitOfWork = new();
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(TrackingData.Now.AddHours(2)));

    private RegisterTrackingEventHandler Handler() => new(_repository, _unitOfWork, _clock);

    private static RegisterTrackingEventCommand Command(string status = "PickedUp", DateTime? occurredAt = null) =>
        new(" pf23 4567 89ab ", status, "CEDIS Monterrey", "Recolectado en domicilio", occurredAt);

    [Fact]
    public async Task Registers_the_movement_using_the_current_time_by_default()
    {
        var tracking = TrackingData.Started();
        _repository.Add(tracking);

        await Handler().Handle(Command(), CancellationToken.None);

        Assert.Equal(TrackingStatus.PickedUp, tracking.CurrentStatus);
        Assert.Equal("CEDIS Monterrey", tracking.CurrentLocation.Name);
        Assert.Equal(_clock.GetUtcNow().UtcDateTime, tracking.LastUpdatedAt);
        Assert.Equal(2, tracking.Events.Count);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Accepts_a_past_date()
    {
        var tracking = TrackingData.Started();
        _repository.Add(tracking);
        var oneHourAgo = TrackingData.Now.AddHours(1);

        await Handler().Handle(Command(occurredAt: oneHourAgo), CancellationToken.None);

        Assert.Equal(oneHourAgo, tracking.LastUpdatedAt);
    }

    [Fact]
    public async Task Rejects_a_future_date()
    {
        _repository.Add(TrackingData.Started());

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            Handler().Handle(Command(occurredAt: _clock.GetUtcNow().UtcDateTime.AddHours(1)), CancellationToken.None));

        Assert.Equal("OccurredAt", ex.ParamName);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Theory]
    [InlineData("Delivered")]       // lo decide Deliveries
    [InlineData("OutForDelivery")]  // lo decide Deliveries
    [InlineData("Cancelled")]       // lo decide Shipments
    public async Task Statuses_owned_by_other_modules_cannot_be_registered_by_hand(string status)
    {
        _repository.Add(TrackingData.Started());

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => Handler().Handle(Command(status), CancellationToken.None));

        Assert.Equal("Status", ex.ParamName);
    }

    [Fact]
    public async Task Invalid_transition_is_a_domain_error()
    {
        _repository.Add(TrackingData.Started());

        // Created → InTransit se salta PickedUp.
        var ex = await Assert.ThrowsAsync<DomainException>(() => Handler().Handle(Command("InTransit"), CancellationToken.None));

        Assert.Equal("tracking.invalid_transition", ex.Code);
    }

    [Fact]
    public async Task Unknown_tracking_number_is_not_found()
    {
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => Handler().Handle(Command(), CancellationToken.None));

        Assert.Equal("tracking.not_found", ex.Code);
    }
}
