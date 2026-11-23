using ParcelFlow.BuildingBlocks.Application.Messaging;

namespace ParcelFlow.Modules.Deliveries.Application.ConfirmDelivery;

/// <summary>DE-004, DE-005, DE-008: registrar un intento exitoso, con quién recibió el paquete.</summary>
public sealed record ConfirmDeliveryCommand(Guid DeliveryId, string ReceivedBy, string? Notes = null) : ICommand<Unit>;
