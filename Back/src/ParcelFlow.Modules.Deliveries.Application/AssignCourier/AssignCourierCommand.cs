using ParcelFlow.BuildingBlocks.Application.Messaging;

namespace ParcelFlow.Modules.Deliveries.Application.AssignCourier;

/// <summary>DE-002: asignar (o reasignar) un repartidor a una entrega.</summary>
public sealed record AssignCourierCommand(Guid DeliveryId, string CourierId) : ICommand<Unit>;
