using ParcelFlow.Modules.Tracking.Application.GetTrackingStats;

namespace ParcelFlow.Modules.Tracking.Application.Abstractions;

/// <summary>Consultas de solo lectura del módulo (ver IShipmentQueries para la explicación).</summary>
public interface ITrackingQueries
{
    Task<TrackingStatsDto> GetStatsAsync(CancellationToken cancellationToken);
}
