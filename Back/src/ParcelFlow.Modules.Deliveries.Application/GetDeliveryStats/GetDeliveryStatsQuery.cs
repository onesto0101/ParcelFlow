using ParcelFlow.BuildingBlocks.Application.Messaging;
using ParcelFlow.Modules.Deliveries.Application.Abstractions;

namespace ParcelFlow.Modules.Deliveries.Application.GetDeliveryStats;

/// <summary>Cuántas entregas hay en cada estado (para el Dashboard).</summary>
public sealed record GetDeliveryStatsQuery : IQuery<DeliveryStatsDto>;

public sealed record DeliveryStatsDto(int Pending, int Assigned, int Delivered, int Failed, int Cancelled);

internal sealed class GetDeliveryStatsHandler(IDeliveryQueries queries) : IQueryHandler<GetDeliveryStatsQuery, DeliveryStatsDto>
{
    public Task<DeliveryStatsDto> Handle(GetDeliveryStatsQuery query, CancellationToken cancellationToken) =>
        queries.GetStatsAsync(cancellationToken);
}
