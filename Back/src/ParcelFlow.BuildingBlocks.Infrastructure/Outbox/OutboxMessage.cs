namespace ParcelFlow.BuildingBlocks.Infrastructure.Outbox;

/// <summary>
/// Un integration event pendiente de publicar, guardado como fila en la tabla "OutboxMessages"
/// del schema de cada módulo (shipments.OutboxMessages, tracking.OutboxMessages...).
///
/// El problema que resuelve ("dual write"): si guardamos el envío y DESPUÉS publicamos el evento,
/// el proceso puede caerse entre las dos cosas → envío guardado, pero Tracking nunca se entera.
/// Con el outbox, el envío y el mensaje se guardan en la MISMA transacción: o los dos o ninguno.
/// Luego <see cref="OutboxProcessor{TDbContext}"/> lee los pendientes y los publica.
/// </summary>
public sealed class OutboxMessage
{
    public const int TypeMaxLength = 300;
    public const int ErrorMaxLength = 2000;

    public Guid Id { get; private set; }

    /// <summary>Tipo .NET del evento ("Namespace.Clase, Ensamblado"), para poder deserializarlo.</summary>
    public string Type { get; private set; } = string.Empty;

    /// <summary>El evento serializado en JSON.</summary>
    public string Content { get; private set; } = string.Empty;

    public DateTime OccurredOnUtc { get; private set; }
    public DateTime? ProcessedOnUtc { get; private set; }
    public int Attempts { get; private set; }
    public string? LastError { get; private set; }

    private OutboxMessage() { }

    internal OutboxMessage(Guid id, string type, string content, DateTime occurredOnUtc)
    {
        Id = id;
        Type = type;
        Content = content;
        OccurredOnUtc = occurredOnUtc;
    }

    internal void MarkAsProcessed(DateTime processedOnUtc)
    {
        ProcessedOnUtc = processedOnUtc;
        LastError = null;
    }

    internal void MarkAsFailed(string error)
    {
        Attempts++;
        LastError = error.Length > ErrorMaxLength ? error[..ErrorMaxLength] : error;
    }
}
