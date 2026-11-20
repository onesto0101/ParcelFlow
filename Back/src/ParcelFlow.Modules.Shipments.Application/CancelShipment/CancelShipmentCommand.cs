using ParcelFlow.BuildingBlocks.Application.Messaging;

namespace ParcelFlow.Modules.Shipments.Application.CancelShipment;

/// <summary>
/// Caso de uso SH-010: cancelar un envío. No devuelve nada útil → ICommand&lt;Unit&gt;.
/// </summary>
public sealed record CancelShipmentCommand(Guid ShipmentId, string Reason) : ICommand<Unit>;
