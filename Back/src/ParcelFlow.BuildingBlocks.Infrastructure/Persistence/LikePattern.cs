namespace ParcelFlow.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Patrón para búsquedas "contiene" con LIKE / ILIKE.
///
/// En LIKE, "%" y "_" son comodines. Si el usuario busca "50%", sin escapar encontraría
/// cualquier cosa que empiece por "50". Se escapan con "\" (el carácter de escape de Postgres).
/// </summary>
public static class LikePattern
{
    public static string Contains(string text)
    {
        var escaped = text
            .Replace(@"\", @"\\")
            .Replace("%", @"\%")
            .Replace("_", @"\_");

        return $"%{escaped}%";
    }
}
