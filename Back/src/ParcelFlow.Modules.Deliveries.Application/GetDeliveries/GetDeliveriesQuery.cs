using ParcelFlow.BuildingBlocks.Application.Messaging;
using ParcelFlow.BuildingBlocks.Application.Paging;

namespace ParcelFlow.Modules.Deliveries.Application.GetDeliveries;

/// <summary>
/// DE-001 (pendientes: status=Pending) y DE-003 (de un repartidor: courierId=REP-014).
/// </summary>
/// <param name="Search">Parte del número de guía.</param>
public sealed record GetDeliveriesQuery(
    string? Status = null,
    string? CourierId = null,
    string? Search = null,
    int? Page = null,
    int? PageSize = null) : IQuery<PagedResult<DeliverySummaryDto>>;

public sealed record DeliverySummaryDto(
    Guid Id,
    Guid ShipmentId,
    string TrackingNumber,
    string Status,
    string? CourierId,
    int AttemptCount,
    DateTime CreatedAt,
    DateTime? AssignedAt,
    DateTime? CompletedAt);
