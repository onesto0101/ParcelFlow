using ParcelFlow.BuildingBlocks.Application.Paging;
using ParcelFlow.Modules.Deliveries.Application.GetDeliveries;
using ParcelFlow.Modules.Deliveries.Application.GetDeliveryStats;
using ParcelFlow.Modules.Deliveries.Domain;

namespace ParcelFlow.Modules.Deliveries.Application.Abstractions;

/// <summary>Consultas de solo lectura del módulo (ver IShipmentQueries para la explicación).</summary>
public interface IDeliveryQueries
{
    Task<PagedResult<DeliverySummaryDto>> ListAsync(
        DeliveryStatus? status, string? courierId, string? search, PageRequest page, CancellationToken cancellationToken);

    Task<DeliveryStatsDto> GetStatsAsync(CancellationToken cancellationToken);
}
