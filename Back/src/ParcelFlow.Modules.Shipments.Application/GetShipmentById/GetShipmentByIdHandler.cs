using ParcelFlow.BuildingBlocks.Application.Messaging;
using ParcelFlow.Modules.Shipments.Application.Abstractions;
using ParcelFlow.Modules.Shipments.Application.GetShipmentByTrackingNumber;

namespace ParcelFlow.Modules.Shipments.Application.GetShipmentById;

internal sealed class GetShipmentByIdHandler(IShipmentRepository repository)
    : IQueryHandler<GetShipmentByIdQuery, ShipmentDto>
{
    public async Task<ShipmentDto> Handle(
        GetShipmentByIdQuery query,
        CancellationToken cancellationToken)
    {
        var shipment = await repository.GetByIdAsync(query.ShipmentId, cancellationToken)
            ?? throw ShipmentNotFound.For(query.ShipmentId);

        return shipment.ToDto();
    }
}