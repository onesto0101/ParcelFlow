using ParcelFlow.Modules.Tracking.Domain;

namespace ParcelFlow.Modules.Tracking.Application.Abstractions;

/// <summary>
/// Repositorio del aggregate ShipmentTracking. Devuelve el aggregate COMPLETO, con todo su
/// historial: el aggregate necesita conocer su estado para validar la siguiente transición.
/// </summary>
public interface IShipmentTrackingRepository
{
    /// <summary>Lo usan los handlers de eventos: los eventos de otros módulos traen el Id del envío.</summary>
    Task<ShipmentTracking?> GetByShipmentIdAsync(Guid shipmentId, CancellationToken cancellationToken);

    /// <summary>Lo usa la API: el operador o el cliente solo tienen el número de guía.</summary>
    Task<ShipmentTracking?> GetByTrackingNumberAsync(string trackingNumber, CancellationToken cancellationToken);

    Task<bool> ExistsForShipmentAsync(Guid shipmentId, CancellationToken cancellationToken);

    void Add(ShipmentTracking tracking);
}
