using ParcelFlow.BuildingBlocks.Application.Paging;
using ParcelFlow.Modules.Shipments.Application.GetShipments;
using ParcelFlow.Modules.Shipments.Application.GetShipmentStats;
using ParcelFlow.Modules.Shipments.Domain;

namespace ParcelFlow.Modules.Shipments.Application.Abstractions;

/// <summary>
/// Consultas de solo lectura (el lado "Q" de CQRS). Diferencias con <see cref="IShipmentRepository"/>:
///
///   Repositorio (escritura)                  Queries (lectura)
///   ─────────────────────────────            ─────────────────────────────────
///   devuelve aggregates completos            devuelve DTOs a medida de la pantalla
///   EF los rastrea para luego guardar        sin rastreo (AsNoTracking): más rápido
///   uno o pocos objetos                      listas, filtros, conteos, paginación
///
/// Así el modelo de dominio no se deforma para servir pantallas (no le añadimos "SenderName"
/// solo porque la tabla lo muestra).
/// </summary>
public interface IShipmentQueries
{
    Task<PagedResult<ShipmentSummaryDto>> ListAsync(
        ShipmentStatus? status, string? search, PageRequest page, CancellationToken cancellationToken);

    Task<ShipmentStatsDto> GetStatsAsync(CancellationToken cancellationToken);
}
