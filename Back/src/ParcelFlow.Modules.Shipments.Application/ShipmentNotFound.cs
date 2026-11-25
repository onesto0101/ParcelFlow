using ParcelFlow.BuildingBlocks.Application.Exceptions;

namespace ParcelFlow.Modules.Shipments.Application;

/// <summary>Los dos casos de uso sobre un envío que buscan por Id: un solo sitio para el error.</summary>
internal static class ShipmentNotFound
{
    public static NotFoundException For(Guid shipmentId) =>
        new("shipment.not_found", $"No existe una envio con Id '{shipmentId}'.");
}
