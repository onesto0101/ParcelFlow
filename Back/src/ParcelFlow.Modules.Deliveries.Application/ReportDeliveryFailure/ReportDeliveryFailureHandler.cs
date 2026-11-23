using ParcelFlow.BuildingBlocks.Application.Messaging;
using ParcelFlow.BuildingBlocks.Application.Parsing;
using ParcelFlow.Modules.Deliveries.Application.Abstractions;
using ParcelFlow.Modules.Deliveries.Domain;

namespace ParcelFlow.Modules.Deliveries.Application.ReportDeliveryFailure;

/// <summary>
/// El dominio decide qué pasa después: si quedan intentos, la entrega vuelve a Pending
/// (hay que reasignar); si era el tercero, queda Failed. El handler no cuenta intentos.
/// </summary>
internal sealed class ReportDeliveryFailureHandler(
    IDeliveryRepository repository,
    IDeliveriesUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<ReportDeliveryFailureCommand, Unit>
{
    public async Task<Unit> Handle(ReportDeliveryFailureCommand command, CancellationToken cancellationToken)
    {
        // Validar la entrada antes de ir a la BD: si el código es inválido, ni siquiera se carga nada.
        var reason = new FailureReason(
            EnumParsing.Parse<DeliveryFailureCode>(command.FailureCode, nameof(command.FailureCode)),
            command.Details);

        var delivery = await repository.GetByIdAsync(command.DeliveryId, cancellationToken)
            ?? throw DeliveryNotFound.For(command.DeliveryId);

        delivery.ReportFailure(reason, timeProvider.GetUtcNow().UtcDateTime);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
