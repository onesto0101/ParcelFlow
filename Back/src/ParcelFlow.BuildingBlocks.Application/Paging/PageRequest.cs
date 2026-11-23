namespace ParcelFlow.BuildingBlocks.Application.Paging;

/// <summary>
/// Página pedida por el cliente, ya normalizada. Nunca se confía en lo que llega:
/// sin un máximo, alguien podría pedir pageSize=1000000 y tumbar la base de datos.
/// </summary>
public readonly record struct PageRequest(int Page, int PageSize)
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    /// <summary>Cuántas filas saltar (OFFSET en SQL).</summary>
    public int Skip => (Page - 1) * PageSize;

    public static PageRequest Create(int? page, int? pageSize) => new(
        Page: Math.Max(page ?? 1, 1),
        PageSize: Math.Clamp(pageSize ?? DefaultPageSize, 1, MaxPageSize));
}
