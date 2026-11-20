using ParcelFlow.BuildingBlocks.Application.Messaging;

namespace ParcelFlow.Modules.Shipments.Application.CreateShipment;

/// <summary>
/// Caso de uso SH-001: registrar un envío.
///
/// Usa tipos primitivos (string, decimal) y no value objects: el comando es la "frontera"
/// con el exterior. Los datos llegan crudos y el handler los convierte en value objects,
/// que es donde se validan.
/// </summary>
public sealed record CreateShipmentCommand(
    ContactInput Sender,
    ContactInput Recipient,
    decimal WeightKg,
    decimal LengthCm,
    decimal WidthCm,
    decimal HeightCm) : ICommand<CreateShipmentResult>;

public sealed record ContactInput(string Name, string Phone, AddressInput Address);

public sealed record AddressInput(string Street, string City, string State, string PostalCode, string Country);

/// <summary>
/// Lo que el front necesita justo después de crear: el Id, el número de guía para
/// mostrarlo al cliente y el costo que calculó el dominio.
/// </summary>
public sealed record CreateShipmentResult(Guid ShipmentId, string TrackingNumber, decimal Cost, string Currency);
