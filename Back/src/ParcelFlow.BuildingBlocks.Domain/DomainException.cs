namespace ParcelFlow.BuildingBlocks.Domain;

/// <summary>
/// Se lanza cuando una operación viola una regla de negocio del estado actual
/// (ej. "no se puede cancelar un envío entregado").
///
/// Diferencia con ArgumentException:
/// - ArgumentException  → el DATO de entrada es inválido (peso negativo, texto vacío).
/// - DomainException    → el dato es válido, pero la OPERACIÓN no está permitida ahora.
///
/// <see cref="Code"/> es estable y legible por máquinas (ej. "shipment.already_delivered"):
/// la API lo devolverá y el front podrá reaccionar a él sin depender del texto del mensaje.
/// </summary>
public class DomainException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
