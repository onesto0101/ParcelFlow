using ParcelFlow.Modules.Shipments.Domain;

namespace ParcelFlow.Modules.Shipments.Application.Abstractions;

/// <summary>
/// "Colección" de envíos. Application define QUÉ necesita; Infrastructure decidirá CÓMO
/// (EF Core + Postgres). Esto es inversión de dependencias: Application no sabe que existe una BD.
///
/// Reglas de un repositorio DDD:
/// - Uno por aggregate root (no hay repositorio de Contact ni de Address).
/// - Devuelve aggregates completos, nunca a medias.
/// - No guarda: solo añade/obtiene. Guardar es trabajo de <see cref="IShipmentsUnitOfWork"/>.
/// </summary>
public interface IShipmentRepository
{
    Task<Shipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Shipment?> GetByTrackingNumberAsync(TrackingNumber trackingNumber, CancellationToken cancellationToken);

    void Add(Shipment shipment);
}
