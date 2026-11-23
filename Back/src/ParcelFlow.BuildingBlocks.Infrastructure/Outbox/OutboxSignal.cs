using Microsoft.EntityFrameworkCore;

namespace ParcelFlow.BuildingBlocks.Infrastructure.Outbox;

/// <summary>Lo único que necesita el interceptor: avisar "hay mensajes nuevos".</summary>
public interface IOutboxSignal
{
    void Notify();
}

/// <summary>
/// "Timbre" entre quien guarda mensajes y el procesador del outbox de UN módulo.
///
/// Sin timbre, el procesador tendría que consultar la tabla cada segundo (mucha carga para nada)
/// o cada mucho tiempo (eventos lentos). Con timbre: al guardar se avisa y el procesador despierta
/// al instante; además revisa cada pocos segundos por si algo quedó pendiente (ej. tras un reinicio).
///
/// Es genérico en el DbContext para que cada módulo tenga su propio timbre (singleton).
/// </summary>
public sealed class OutboxSignal<TDbContext> : IOutboxSignal, IDisposable
    where TDbContext : DbContext
{
    // Máximo 1: diez avisos seguidos equivalen a uno ("hay trabajo"), no a diez vueltas.
    private readonly SemaphoreSlim _semaphore = new(initialCount: 0, maxCount: 1);

    public void Notify()
    {
        try
        {
            _semaphore.Release();
        }
        catch (SemaphoreFullException)
        {
            // Ya había un aviso pendiente: no hace falta otro.
        }
    }

    /// <summary>Espera un aviso o a que pase <paramref name="timeout"/>, lo que ocurra antes.</summary>
    public Task WaitAsync(TimeSpan timeout, CancellationToken cancellationToken) =>
        _semaphore.WaitAsync(timeout, cancellationToken);

    public void Dispose() => _semaphore.Dispose();
}
