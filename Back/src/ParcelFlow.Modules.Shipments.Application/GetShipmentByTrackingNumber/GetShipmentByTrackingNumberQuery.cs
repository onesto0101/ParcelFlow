using ParcelFlow.BuildingBlocks.Application.Messaging;

namespace ParcelFlow.Modules.Shipments.Application.GetShipmentByTrackingNumber;

/// <summary>
/// Consulta: obtener un envío por su número de guía (lo que el cliente tiene en la mano).
/// </summary>
public sealed record GetShipmentByTrackingNumberQuery(string TrackingNumber) : IQuery<ShipmentDto>;

/// <summary>
/// DTO (Data Transfer Object): la "foto" del envío que sale hacia la API.
/// Nunca devolvemos la entidad Shipment: tiene comportamiento, eventos y tipos del dominio
/// que el exterior no debe ver ni poder modificar. Si mañana cambia el dominio, el contrato
/// con el front sigue igual.
/// </summary>
public sealed record ShipmentDto(
    Guid Id,
    string TrackingNumber,
    string Status,
    ContactDto Sender,
    ContactDto Recipient,
    decimal WeightKg,
    decimal LengthCm,
    decimal WidthCm,
    decimal HeightCm,
    decimal Cost,
    string Currency,
    DateTime CreatedAt,
    DateTime? CancelledAt,
    string? CancellationReason,
    DateTime? DeliveredAt);

public sealed record ContactDto(string Name, string Phone, AddressDto Address);

public sealed record AddressDto(string Street, string City, string State, string PostalCode, string Country);
