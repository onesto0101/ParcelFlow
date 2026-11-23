using ParcelFlow.BuildingBlocks.Application.Messaging;
using ParcelFlow.Modules.Shipments.Application.Abstractions;

namespace ParcelFlow.Modules.Shipments.Application.GetShipmentStats;

/// <summary>Cuántos envíos hay en cada estado (para el Dashboard).</summary>
public sealed record GetShipmentStatsQuery : IQuery<ShipmentStatsDto>;

public sealed record ShipmentStatsDto(int Total, int Registered, int Delivered, int Cancelled);

internal sealed class GetShipmentStatsHandler(IShipmentQueries queries) : IQueryHandler<GetShipmentStatsQuery, ShipmentStatsDto>
{
    public Task<ShipmentStatsDto> Handle(GetShipmentStatsQuery query, CancellationToken cancellationToken) =>
        queries.GetStatsAsync(cancellationToken);
}
