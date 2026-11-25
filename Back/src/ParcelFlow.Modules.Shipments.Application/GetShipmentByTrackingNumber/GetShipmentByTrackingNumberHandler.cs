using ParcelFlow.BuildingBlocks.Application.Exceptions;
using ParcelFlow.BuildingBlocks.Application.Messaging;
using ParcelFlow.Modules.Shipments.Application.Abstractions;
using ParcelFlow.Modules.Shipments.Domain;

namespace ParcelFlow.Modules.Shipments.Application.GetShipmentByTrackingNumber;

/// <summary>
/// Lee el envío y lo convierte a DTO. No modifica nada y no llama a SaveChanges.
///
/// Simplificación consciente: usamos el repositorio (carga el aggregate completo).
/// En un CQRS "más fuerte" las consultas irían directo a la BD con una proyección
/// (SELECT solo de las columnas necesarias). Lo cambiaremos si hace falta.
/// </summary>
internal sealed class GetShipmentByTrackingNumberHandler(IShipmentRepository repository)
    : IQueryHandler<GetShipmentByTrackingNumberQuery, ShipmentDto>
{
    public async Task<ShipmentDto> Handle(GetShipmentByTrackingNumberQuery query, CancellationToken cancellationToken)
    {
        // From valida el formato: un número mal escrito da ArgumentException (400), no un 404.
        var trackingNumber = TrackingNumber.From(query.TrackingNumber);

        var shipment = await repository.GetByTrackingNumberAsync(trackingNumber, cancellationToken)
            ?? throw new NotFoundException(
                "shipment.not_found",
                $"No existe un envío con número de guía '{trackingNumber}'.");

        return shipment.ToDto();
    }

}
