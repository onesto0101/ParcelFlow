using ParcelFlow.BuildingBlocks.Application.Messaging;
using ParcelFlow.BuildingBlocks.Application.Paging;
using ParcelFlow.BuildingBlocks.Application.Parsing;
using ParcelFlow.Modules.Deliveries.Application.Abstractions;
using ParcelFlow.Modules.Deliveries.Domain;

namespace ParcelFlow.Modules.Deliveries.Application.GetDeliveries;

internal sealed class GetDeliveriesHandler(IDeliveryQueries queries)
    : IQueryHandler<GetDeliveriesQuery, PagedResult<DeliverySummaryDto>>
{
    public const int SearchMaxLength = 50;

    public Task<PagedResult<DeliverySummaryDto>> Handle(GetDeliveriesQuery query, CancellationToken cancellationToken)
    {
        var status = EnumParsing.ParseOptional<DeliveryStatus>(query.Status, nameof(query.Status));

        // Reutiliza la validación del value object: un CourierId inválido da 400, no "0 resultados".
        var courierId = string.IsNullOrWhiteSpace(query.CourierId) ? null : new CourierId(query.CourierId).Value;

        var search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();
        if (search?.Length > SearchMaxLength)
            throw new ArgumentException($"La búsqueda admite como máximo {SearchMaxLength} caracteres.", nameof(query.Search));

        return queries.ListAsync(status, courierId, search, PageRequest.Create(query.Page, query.PageSize), cancellationToken);
    }
}
