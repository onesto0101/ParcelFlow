using ParcelFlow.BuildingBlocks.Application.Exceptions;
using ParcelFlow.BuildingBlocks.Application.Messaging;
using ParcelFlow.Modules.Shipments.Application.Abstractions;

namespace ParcelFlow.Modules.Shipments.Application.CancelShipment;

/// <summary>
/// Patrón típico de un comando sobre un aggregate existente:
///   cargar → llamar a un método del dominio → guardar.
/// Las reglas ("no se puede cancelar un envío entregado") están en Shipment.Cancel, no aquí.
/// </summary>
internal sealed class CancelShipmentHandler(
    IShipmentRepository repository,
    IShipmentsUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<CancelShipmentCommand, Unit>
{
    public async Task<Unit> Handle(CancelShipmentCommand command, CancellationToken cancellationToken)
    {
        var shipment = await repository.GetByIdAsync(command.ShipmentId, cancellationToken)
            ?? throw new NotFoundException(
                "shipment.not_found",
                $"No existe un envío con Id '{command.ShipmentId}'.");

        shipment.Cancel(command.Reason, timeProvider.GetUtcNow().UtcDateTime);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
