using Microsoft.Extensions.Time.Testing;
using ParcelFlow.BuildingBlocks.Application.Exceptions;
using ParcelFlow.BuildingBlocks.Domain;
using ParcelFlow.Modules.Deliveries.Application.AssignCourier;
using ParcelFlow.Modules.Deliveries.Application.ConfirmDelivery;
using ParcelFlow.Modules.Deliveries.Application.GetDeliveryById;
using ParcelFlow.Modules.Deliveries.Application.ReportDeliveryFailure;
using ParcelFlow.Modules.Deliveries.Domain;

namespace ParcelFlow.Modules.Deliveries.UnitTests.Application;

/// <summary>
/// Los handlers son finos (cargar → dominio → guardar), así que los tests comprueban sobre todo
/// el "pegamento": que conviertan bien la entrada, que guarden, y los errores de la aplicación.
/// Las reglas (máximo 3 intentos, etc.) ya están probadas en DeliveryTests.
/// </summary>
public class DeliveryCommandHandlersTests
{
    private static readonly DateTime Now = new(2026, 11, 23, 10, 0, 0, DateTimeKind.Utc);

    private readonly FakeDeliveryRepository _repository = new();
    private readonly FakeDeliveriesUnitOfWork _unitOfWork = new();
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(Now.AddHours(1)));

    private Delivery Pending()
    {
        var delivery = Delivery.CreatePending(Guid.NewGuid(), "PF23456789AB", Now);
        _repository.Add(delivery);
        return delivery;
    }

    private Task Assign(Delivery delivery, string courier = "REP-014") =>
        new AssignCourierHandler(_repository, _unitOfWork, _clock).Handle(new AssignCourierCommand(delivery.Id, courier), CancellationToken.None);

    [Fact]
    public async Task Assign_sets_courier_and_saves()
    {
        var delivery = Pending();

        await Assign(delivery);

        Assert.Equal(DeliveryStatus.Assigned, delivery.Status);
        Assert.Equal("REP-014", delivery.CourierId?.Value);
        Assert.Equal(_clock.GetUtcNow().UtcDateTime, delivery.AssignedAt);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Assign_unknown_delivery_is_not_found()
    {
        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            new AssignCourierHandler(_repository, _unitOfWork, _clock).Handle(new AssignCourierCommand(Guid.NewGuid(), "REP-014"), CancellationToken.None));

        Assert.Equal("delivery.not_found", ex.Code);
    }

    [Fact]
    public async Task Confirm_records_who_received_it()
    {
        var delivery = Pending();
        await Assign(delivery);

        await new ConfirmDeliveryHandler(_repository, _unitOfWork, _clock)
            .Handle(new ConfirmDeliveryCommand(delivery.Id, "Luis Pérez", "Dejado en recepción"), CancellationToken.None);

        Assert.Equal(DeliveryStatus.Delivered, delivery.Status);
        var attempt = Assert.Single(delivery.Attempts);
        Assert.Equal("Luis Pérez", attempt.Evidence?.ReceivedBy);
    }

    [Fact]
    public async Task Confirm_without_courier_is_a_domain_error()
    {
        var delivery = Pending();

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            new ConfirmDeliveryHandler(_repository, _unitOfWork, _clock)
                .Handle(new ConfirmDeliveryCommand(delivery.Id, "Luis Pérez"), CancellationToken.None));

        Assert.Equal("delivery.not_assigned", ex.Code);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Theory]
    [InlineData("RecipientAbsent", DeliveryFailureCode.RecipientAbsent)]
    [InlineData("wrongaddress", DeliveryFailureCode.WrongAddress)]
    public async Task Fail_parses_the_failure_code(string code, DeliveryFailureCode expected)
    {
        var delivery = Pending();
        await Assign(delivery);

        await new ReportDeliveryFailureHandler(_repository, _unitOfWork, _clock)
            .Handle(new ReportDeliveryFailureCommand(delivery.Id, code), CancellationToken.None);

        Assert.Equal(DeliveryStatus.Pending, delivery.Status); // quedan intentos → hay que reasignar
        Assert.Equal(expected, delivery.Attempts.Single().FailureReason?.Code);
    }

    [Fact]
    public async Task Fail_with_unknown_code_is_rejected_before_touching_the_database()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            new ReportDeliveryFailureHandler(_repository, _unitOfWork, _clock)
                .Handle(new ReportDeliveryFailureCommand(Guid.NewGuid(), "DogAteIt"), CancellationToken.None));

        Assert.Equal("FailureCode", ex.ParamName);
    }

    [Fact]
    public async Task Detail_exposes_which_actions_are_allowed()
    {
        var delivery = Pending();
        var handler = new GetDeliveryByIdHandler(_repository);

        var pending = await handler.Handle(new GetDeliveryByIdQuery(delivery.Id), CancellationToken.None);
        Assert.True(pending.CanAssign);
        Assert.False(pending.CanRegisterAttempt);

        await Assign(delivery);
        var assigned = await handler.Handle(new GetDeliveryByIdQuery(delivery.Id), CancellationToken.None);
        Assert.True(assigned.CanAssign); // reasignar sigue permitido
        Assert.True(assigned.CanRegisterAttempt);
        Assert.Equal("REP-014", assigned.CourierId);
    }
}
