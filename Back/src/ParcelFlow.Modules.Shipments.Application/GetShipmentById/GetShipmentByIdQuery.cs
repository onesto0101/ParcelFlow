using ParcelFlow.BuildingBlocks.Application.Messaging;
using ParcelFlow.Modules.Shipments.Application.GetShipmentByTrackingNumber;

namespace ParcelFlow.Modules.Shipments.Application.GetShipmentById;

public sealed record GetShipmentByIdQuery(Guid ShipmentId) : IQuery<ShipmentDto>;


