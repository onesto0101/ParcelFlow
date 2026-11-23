using System.Text.Json;
using ParcelFlow.BuildingBlocks.Application.IntegrationEvents;

namespace ParcelFlow.BuildingBlocks.Infrastructure.Outbox;

/// <summary>
/// Convierte un integration event en (tipo, JSON) para guardarlo, y al revés para publicarlo.
/// </summary>
internal static class OutboxSerializer
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static OutboxMessage ToMessage(IIntegrationEvent integrationEvent)
    {
        var type = integrationEvent.GetType();

        // "Namespace.Clase, Ensamblado" SIN versión: si sube la versión del ensamblado,
        // los mensajes que ya estaban guardados se siguen pudiendo leer.
        var typeName = $"{type.FullName}, {type.Assembly.GetName().Name}";

        return new OutboxMessage(
            Guid.CreateVersion7(),
            typeName,
            JsonSerializer.Serialize(integrationEvent, type, Options),
            integrationEvent.OccurredOnUtc);
    }

    public static IIntegrationEvent FromMessage(OutboxMessage message)
    {
        var type = Type.GetType(message.Type, throwOnError: true)!;

        return (IIntegrationEvent)(JsonSerializer.Deserialize(message.Content, type, Options)
            ?? throw new InvalidOperationException($"El mensaje {message.Id} está vacío."));
    }
}
