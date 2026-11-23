namespace ParcelFlow.BuildingBlocks.Application.Paging;

/// <summary>
/// Una "página" de resultados + lo necesario para pintar el paginador en el front.
/// </summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
