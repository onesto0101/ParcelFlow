using ParcelFlow.BuildingBlocks.Application.Messaging;
using ParcelFlow.Modules.Deliveries.Application.Abstractions;
using ParcelFlow.Modules.Deliveries.Domain;

namespace ParcelFlow.Modules.Deliveries.Application.AssignCourier;

/// <summary>
/// cargar → dominio → guardar. Al guardar, el outbox publica DeliveryAssigned y Tracking
/// pone el paquete "En reparto". Este handler no sabe nada de Tracking.
/// </summary>
internal sealed class AssignCourierHandler(
    IDeliveryRepository repository,
    IDeliveriesUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<AssignCourierCommand, Unit>
{
    public async Task<Unit> Handle(AssignCourierCommand command, CancellationToken cancellationToken)
    {
        var delivery = await repository.GetByIdAsync(command.DeliveryId, cancellationToken)
            ?? throw DeliveryNotFound.For(command.DeliveryId);

        delivery.AssignCourier(new CourierId(command.CourierId), timeProvider.GetUtcNow().UtcDateTime);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
