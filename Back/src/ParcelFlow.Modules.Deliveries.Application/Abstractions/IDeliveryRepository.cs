using ParcelFlow.Modules.Deliveries.Domain;

namespace ParcelFlow.Modules.Deliveries.Application.Abstractions;

/// <summary>Repositorio del aggregate Delivery (siempre con sus intentos).</summary>
public interface IDeliveryRepository
{
    Task<Delivery?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Delivery?> GetByShipmentIdAsync(Guid shipmentId, CancellationToken cancellationToken);

    Task<bool> ExistsForShipmentAsync(Guid shipmentId, CancellationToken cancellationToken);

    void Add(Delivery delivery);
}
