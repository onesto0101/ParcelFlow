using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ParcelFlow.BuildingBlocks.Infrastructure.Outbox;

/// <summary>
/// Proceso en segundo plano (BackgroundService) que publica el outbox de UN módulo.
/// Hay uno por módulo: OutboxProcessor&lt;ShipmentsDbContext&gt;, OutboxProcessor&lt;TrackingDbContext&gt;...
///
/// Bucle: esperar el timbre (o <see cref="PollingInterval"/>) → leer los pendientes en orden →
/// publicar cada uno → marcarlo como procesado → guardar.
///
/// Garantías y límites de esta v1:
/// - "Al menos una vez": si se cae justo después de publicar y antes de marcar, se republica.
/// - Orden: se procesan por fecha y se PARA en el primer fallo, para no publicar un
///   "ShipmentCancelled" antes que el "ShipmentCreated" que falló.
/// - Un mensaje que falla <see cref="MaxAttempts"/> veces se aparta (queda con LastError en la
///   tabla para revisarlo a mano) y deja pasar a los siguientes.
/// - Pensado para UNA instancia de la API. Con varias, habría que bloquear filas
///   (SELECT ... FOR UPDATE SKIP LOCKED) para que dos procesadores no publiquen lo mismo.
/// </summary>
public sealed class OutboxProcessor<TDbContext>(
    IServiceScopeFactory scopeFactory,
    OutboxSignal<TDbContext> signal,
    TimeProvider timeProvider,
    ILogger<OutboxProcessor<TDbContext>> logger) : BackgroundService
    where TDbContext : DbContext
{
    public const int BatchSize = 20;
    public const int MaxAttempts = 5;
    public static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Devuelve el control enseguida para no retrasar el arranque de la app.
        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            var hasMore = false;

            try
            {
                hasMore = await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // Ej. la BD no responde. No se detiene: lo vuelve a intentar en la siguiente vuelta.
                logger.LogError(ex, "Error procesando el outbox de {DbContext}", typeof(TDbContext).Name);
            }

            if (hasMore) continue;

            try
            {
                await signal.WaitAsync(PollingInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    /// <returns>true si seguramente quedan más mensajes (lote lleno y sin fallos).</returns>
    private async Task<bool> ProcessBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

        var messages = await dbContext.Set<OutboxMessage>()
            .Where(m => m.ProcessedOnUtc == null && m.Attempts < MaxAttempts)
            .OrderBy(m => m.OccurredOnUtc)
            .ThenBy(m => m.Id)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        var failed = false;

        foreach (var message in messages)
        {
            try
            {
                var integrationEvent = OutboxSerializer.FromMessage(message);
                await eventBus.PublishAsync(integrationEvent, cancellationToken);
                message.MarkAsProcessed(timeProvider.GetUtcNow().UtcDateTime);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                message.MarkAsFailed(ex.ToString());
                logger.LogWarning(ex, "Falló el mensaje {MessageId} ({Type}), intento {Attempt} de {Max}",
                    message.Id, message.Type, message.Attempts, MaxAttempts);
                failed = true;
                break; // conservar el orden: los siguientes esperan a la próxima vuelta
            }
        }

        if (messages.Count > 0)
            await dbContext.SaveChangesAsync(cancellationToken);

        return !failed && messages.Count == BatchSize;
    }
}
