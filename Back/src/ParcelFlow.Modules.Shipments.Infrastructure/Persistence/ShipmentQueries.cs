using Microsoft.EntityFrameworkCore;
using ParcelFlow.BuildingBlocks.Application.Paging;
using ParcelFlow.BuildingBlocks.Infrastructure.Persistence;
using ParcelFlow.Modules.Shipments.Application.Abstractions;
using ParcelFlow.Modules.Shipments.Application.GetShipments;
using ParcelFlow.Modules.Shipments.Application.GetShipmentStats;
using ParcelFlow.Modules.Shipments.Domain;

namespace ParcelFlow.Modules.Shipments.Infrastructure.Persistence;

/// <summary>
/// Implementación de las consultas de lectura con EF Core.
///
/// - AsNoTracking: EF no guarda copia de lo leído para detectar cambios (no vamos a guardar nada).
/// - Select(...) = proyección: el SQL trae solo las columnas de la fila, no el aggregate entero.
/// Pon "Microsoft.EntityFrameworkCore.Database.Command" en Information (ya está en Development)
/// y mira el SQL en la consola.
/// </summary>
internal sealed class ShipmentQueries(ShipmentsDbContext dbContext) : IShipmentQueries
{
    public async Task<PagedResult<ShipmentSummaryDto>> ListAsync(
        ShipmentStatus? status, string? search, PageRequest page, CancellationToken cancellationToken)
    {
        var query = dbContext.Shipments.AsNoTracking();

        if (status is not null)
            query = query.Where(s => s.Status == status);

        if (search is not null)
        {
            var pattern = LikePattern.Contains(search);

            // ILIKE = LIKE sin distinguir mayúsculas (propio de Postgres).
            // (string)(object)s.TrackingNumber: TrackingNumber es un value object guardado como texto
            // (value converter). El doble cast le dice a EF "trátalo como la columna de texto".
            query = query.Where(s =>
                EF.Functions.ILike((string)(object)s.TrackingNumber, pattern) ||
                EF.Functions.ILike(s.Sender.Name, pattern) ||
                EF.Functions.ILike(s.Recipient.Name, pattern) ||
                EF.Functions.ILike(s.Sender.Address.City, pattern) ||
                EF.Functions.ILike(s.Recipient.Address.City, pattern));
        }

        // Dos consultas: el total (para el paginador) y la página en sí.
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .ThenByDescending(s => s.Id) // desempate estable: sin él, una fila podría saltar de página
            .Skip(page.Skip)
            .Take(page.PageSize)
            .Select(s => new ShipmentSummaryDto(
                s.Id,
                s.TrackingNumber.Value,
                s.Status.ToString(),
                s.Sender.Name,
                s.Sender.Address.City,
                s.Recipient.Name,
                s.Recipient.Address.City,
                s.Cost.Amount,
                s.Cost.Currency,
                s.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<ShipmentSummaryDto>(items, page.Page, page.PageSize, totalCount);
    }

    public async Task<ShipmentStatsDto> GetStatsAsync(CancellationToken cancellationToken)
    {
        // SELECT "Status", COUNT(*) FROM shipments."Shipments" GROUP BY "Status"
        var counts = await dbContext.Shipments
            .GroupBy(s => s.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);

        int Count(ShipmentStatus status) => counts.GetValueOrDefault(status);

        return new ShipmentStatsDto(
            Total: counts.Values.Sum(),
            Registered: Count(ShipmentStatus.Registered),
            Delivered: Count(ShipmentStatus.Delivered),
            Cancelled: Count(ShipmentStatus.Cancelled));
    }
}
