using ParcelFlow.BuildingBlocks.Application.Messaging;

namespace ParcelFlow.Modules.Tracking.Application.GetTracking;

/// <summary>TR-002 y TR-003: estado actual + historial completo de una guía.</summary>
public sealed record GetTrackingByTrackingNumberQuery(string TrackingNumber) : IQuery<TrackingDto>;

/// <param name="NextManualStatuses">
/// Qué movimientos puede registrar el operador ahora mismo. El front pinta solo esos botones,
/// así no hay que duplicar la máquina de estados en TypeScript.
/// </param>
/// <param name="Events">Historial en orden cronológico (el más antiguo primero).</param>
public sealed record TrackingDto(
    Guid ShipmentId,
    string TrackingNumber,
    string CurrentStatus,
    string CurrentLocation,
    DateTime LastUpdatedAt,
    bool IsFinal,
    IReadOnlyList<string> NextManualStatuses,
    IReadOnlyList<TrackingEventDto> Events);

public sealed record TrackingEventDto(Guid Id, string Status, string Location, string Description, DateTime OccurredAt);
