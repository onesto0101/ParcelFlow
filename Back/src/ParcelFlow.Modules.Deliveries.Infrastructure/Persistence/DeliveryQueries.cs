using Microsoft.EntityFrameworkCore;
using ParcelFlow.BuildingBlocks.Application.Paging;
using ParcelFlow.BuildingBlocks.Infrastructure.Persistence;
using ParcelFlow.Modules.Deliveries.Application.Abstractions;
using ParcelFlow.Modules.Deliveries.Application.GetDeliveries;
using ParcelFlow.Modules.Deliveries.Application.GetDeliveryStats;
using ParcelFlow.Modules.Deliveries.Domain;

namespace ParcelFlow.Modules.Deliveries.Infrastructure.Persistence;

internal sealed class DeliveryQueries(DeliveriesDbContext dbContext) : IDeliveryQueries
{
    public async Task<PagedResult<DeliverySummaryDto>> ListAsync(
        DeliveryStatus? status, string? courierId, string? search, PageRequest page, CancellationToken cancellationToken)
    {
        var query = dbContext.Deliveries.AsNoTracking();

        if (status is not null)
            query = query.Where(d => d.Status == status);

        if (courierId is not null)
        {
            var courier = new CourierId(courierId);
            query = query.Where(d => d.CourierId == courier); // EF aplica el converter al parámetro
        }

        if (search is not null)
        {
            var pattern = LikePattern.Contains(search);
            query = query.Where(d => EF.Functions.ILike(d.TrackingNumber, pattern));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            // Las pendientes más antiguas primero: es la cola de trabajo del despachador (FIFO).
            .OrderBy(d => d.CreatedAt)
            .ThenBy(d => d.Id)
            .Skip(page.Skip)
            .Take(page.PageSize)
            .Select(d => new DeliverySummaryDto(
                d.Id,
                d.ShipmentId,
                d.TrackingNumber,
                d.Status.ToString(),
                d.CourierId == null ? null : d.CourierId.Value,
                d.Attempts.Count, // se traduce a un COUNT(*) en subconsulta, sin cargar los intentos
                d.CreatedAt,
                d.AssignedAt,
                d.CompletedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<DeliverySummaryDto>(items, page.Page, page.PageSize, totalCount);
    }

    public async Task<DeliveryStatsDto> GetStatsAsync(CancellationToken cancellationToken)
    {
        var counts = await dbContext.Deliveries
            .GroupBy(d => d.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);

        int Count(DeliveryStatus status) => counts.GetValueOrDefault(status);

        return new DeliveryStatsDto(
            Pending: Count(DeliveryStatus.Pending),
            Assigned: Count(DeliveryStatus.Assigned),
            Delivered: Count(DeliveryStatus.Delivered),
            Failed: Count(DeliveryStatus.Failed),
            Cancelled: Count(DeliveryStatus.Cancelled));
    }
}
