using ParcelFlow.BuildingBlocks.Application.Exceptions;
using ParcelFlow.BuildingBlocks.Application.Messaging;
using ParcelFlow.Modules.Tracking.Application.Abstractions;
using ParcelFlow.Modules.Tracking.Domain;

namespace ParcelFlow.Modules.Tracking.Application.GetTracking;

/// <summary>
/// Usa el repositorio (aggregate completo) en vez de una proyección, igual que el detalle de
/// Shipments: aquí necesitamos TODO el aggregate (estado + historial) y además su lógica
/// (TrackingStatusTransitions) para calcular los siguientes estados posibles.
/// </summary>
internal sealed class GetTrackingByTrackingNumberHandler(IShipmentTrackingRepository repository)
    : IQueryHandler<GetTrackingByTrackingNumberQuery, TrackingDto>
{
    public async Task<TrackingDto> Handle(GetTrackingByTrackingNumberQuery query, CancellationToken cancellationToken)
    {
        var trackingNumber = TrackingNumberText.Normalize(query.TrackingNumber);

        var tracking = await repository.GetByTrackingNumberAsync(trackingNumber, cancellationToken)
            ?? throw new NotFoundException(
                "tracking.not_found",
                $"No hay seguimiento para la guía '{trackingNumber}'.");

        return new TrackingDto(
            tracking.ShipmentId,
            tracking.TrackingNumber,
            tracking.CurrentStatus.ToString(),
            tracking.CurrentLocation.Name,
            tracking.LastUpdatedAt,
            TrackingStatusTransitions.IsFinal(tracking.CurrentStatus),
            ManualTrackingStatuses.NextFrom(tracking.CurrentStatus).Select(s => s.ToString()).ToList(),
            tracking.Events
                .OrderBy(e => e.OccurredAt)
                .Select(e => new TrackingEventDto(e.Id, e.Status.ToString(), e.Location.Name, e.Description.Value, e.OccurredAt))
                .ToList());
    }
}
