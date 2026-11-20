namespace ParcelFlow.BuildingBlocks.Application.Exceptions;

/// <summary>
/// Se lanza cuando un caso de uso busca algo que no existe (ej. un envío con ese Id).
///
/// Vive en Application y no en Domain porque "no encontrado" no es una regla de negocio:
/// el dominio nunca busca nada, solo trabaja con objetos que ya tiene en la mano.
/// La API la convertirá en un HTTP 404.
/// </summary>
public sealed class NotFoundException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
