using ParcelFlow.BuildingBlocks.Application.Exceptions;

namespace ParcelFlow.Modules.Deliveries.Application;

/// <summary>Los cuatro casos de uso sobre una entrega buscan por Id: un solo sitio para el error.</summary>
internal static class DeliveryNotFound
{
    public static NotFoundException For(Guid deliveryId) =>
        new("delivery.not_found", $"No existe una entrega con Id '{deliveryId}'.");
}
