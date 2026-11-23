using ParcelFlow.BuildingBlocks.Application.Messaging;
using ParcelFlow.Modules.Deliveries.Application.Abstractions;
using ParcelFlow.Modules.Deliveries.Domain;

namespace ParcelFlow.Modules.Deliveries.Application.ConfirmDelivery;

/// <summary>
/// Al guardar se publica ShipmentDelivered: Tracking → Delivered y Shipments → Delivered.
/// Un solo comando aquí, tres módulos actualizados, y ninguno conoce a los otros.
/// </summary>
internal sealed class ConfirmDeliveryHandler(
    IDeliveryRepository repository,
    IDeliveriesUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<ConfirmDeliveryCommand, Unit>
{
    public async Task<Unit> Handle(ConfirmDeliveryCommand command, CancellationToken cancellationToken)
    {
        var delivery = await repository.GetByIdAsync(command.DeliveryId, cancellationToken)
            ?? throw DeliveryNotFound.For(command.DeliveryId);

        delivery.ConfirmDelivery(
            new DeliveryEvidence(command.ReceivedBy, command.Notes),
            timeProvider.GetUtcNow().UtcDateTime);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
