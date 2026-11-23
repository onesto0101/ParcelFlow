using Microsoft.EntityFrameworkCore;
using ParcelFlow.Modules.Tracking.Application.Abstractions;
using ParcelFlow.Modules.Tracking.Application.GetTrackingStats;
using ParcelFlow.Modules.Tracking.Domain;

namespace ParcelFlow.Modules.Tracking.Infrastructure.Persistence;

internal sealed class TrackingQueries(TrackingDbContext dbContext) : ITrackingQueries
{
    public async Task<TrackingStatsDto> GetStatsAsync(CancellationToken cancellationToken)
    {
        var counts = await dbContext.ShipmentTrackings
            .GroupBy(t => t.CurrentStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);

        int Count(TrackingStatus status) => counts.GetValueOrDefault(status);

        return new TrackingStatsDto(
            Created: Count(TrackingStatus.Created),
            PickedUp: Count(TrackingStatus.PickedUp),
            InTransit: Count(TrackingStatus.InTransit),
            AtDistributionCenter: Count(TrackingStatus.AtDistributionCenter),
            OutForDelivery: Count(TrackingStatus.OutForDelivery),
            Delivered: Count(TrackingStatus.Delivered),
            DeliveryFailed: Count(TrackingStatus.DeliveryFailed),
            Cancelled: Count(TrackingStatus.Cancelled));
    }
}
