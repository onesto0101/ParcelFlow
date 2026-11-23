using ParcelFlow.BuildingBlocks.Application.Messaging;
using ParcelFlow.BuildingBlocks.Application.Paging;

namespace ParcelFlow.Modules.Shipments.Application.GetShipments;

/// <summary>
/// Listado de envíos para la tabla de la página "Envíos": filtros + paginación.
/// Todos los filtros son opcionales; llegan tal cual desde la URL (?status=Registered&amp;search=mty).
/// </summary>
/// <param name="Status">Registered, Delivered o Cancelled. Null = todos.</param>
/// <param name="Search">Texto libre: guía, nombre o ciudad del remitente o del destinatario.</param>
public sealed record GetShipmentsQuery(
    string? Status = null,
    string? Search = null,
    int? Page = null,
    int? PageSize = null) : IQuery<PagedResult<ShipmentSummaryDto>>;

/// <summary>
/// Una FILA de la tabla: solo las columnas que se muestran. El detalle completo es
/// ShipmentDto (GetShipmentByTrackingNumber). Lecturas distintas → DTOs distintos.
/// </summary>
public sealed record ShipmentSummaryDto(
    Guid Id,
    string TrackingNumber,
    string Status,
    string SenderName,
    string OriginCity,
    string RecipientName,
    string DestinationCity,
    decimal Cost,
    string Currency,
    DateTime CreatedAt);
