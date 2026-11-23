using ParcelFlow.BuildingBlocks.Application.Messaging;
using ParcelFlow.Modules.Tracking.Application.Abstractions;

namespace ParcelFlow.Modules.Tracking.Application.GetTrackingStats;

/// <summary>Cuántos paquetes hay en cada estado de seguimiento (para el Dashboard).</summary>
public sealed record GetTrackingStatsQuery : IQuery<TrackingStatsDto>;

public sealed record TrackingStatsDto(
    int Created,
    int PickedUp,
    int InTransit,
    int AtDistributionCenter,
    int OutForDelivery,
    int Delivered,
    int DeliveryFailed,
    int Cancelled);

internal sealed class GetTrackingStatsHandler(ITrackingQueries queries) : IQueryHandler<GetTrackingStatsQuery, TrackingStatsDto>
{
    public Task<TrackingStatsDto> Handle(GetTrackingStatsQuery query, CancellationToken cancellationToken) =>
        queries.GetStatsAsync(cancellationToken);
}
