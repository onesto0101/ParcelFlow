using ParcelFlow.BuildingBlocks.Application.Messaging;
using ParcelFlow.BuildingBlocks.Application.Paging;
using ParcelFlow.BuildingBlocks.Application.Parsing;
using ParcelFlow.Modules.Shipments.Application.Abstractions;
using ParcelFlow.Modules.Shipments.Domain;

namespace ParcelFlow.Modules.Shipments.Application.GetShipments;

/// <summary>
/// Lado de LECTURA de CQRS "de verdad": no usa el repositorio ni carga aggregates.
/// Pide a <see cref="IShipmentQueries"/> una proyección (SELECT solo de las columnas de la fila).
///
/// El handler solo valida y normaliza la entrada; el "cómo" (SQL, índices) es de Infrastructure.
/// </summary>
internal sealed class GetShipmentsHandler(IShipmentQueries queries)
    : IQueryHandler<GetShipmentsQuery, PagedResult<ShipmentSummaryDto>>
{
    public const int SearchMaxLength = 100;

    public Task<PagedResult<ShipmentSummaryDto>> Handle(GetShipmentsQuery query, CancellationToken cancellationToken)
    {
        var status = EnumParsing.ParseOptional<ShipmentStatus>(query.Status, nameof(query.Status));

        var search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();
        if (search?.Length > SearchMaxLength)
            throw new ArgumentException($"La búsqueda admite como máximo {SearchMaxLength} caracteres.", nameof(query.Search));

        return queries.ListAsync(status, search, PageRequest.Create(query.Page, query.PageSize), cancellationToken);
    }
}
