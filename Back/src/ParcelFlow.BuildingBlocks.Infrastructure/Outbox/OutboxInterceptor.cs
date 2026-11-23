using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ParcelFlow.BuildingBlocks.Domain;

namespace ParcelFlow.BuildingBlocks.Infrastructure.Outbox;

/// <summary>
/// Se "engancha" a SaveChanges del DbContext de un módulo (interceptor de EF Core):
///
///   ANTES de guardar   → recoge los domain events de los aggregates rastreados, los traduce con
///                        el mapper y los añade como filas OutboxMessage (misma transacción).
///   DESPUÉS de guardar → toca el timbre para que el procesador los publique ya.
///
/// Así ni los handlers ni los repositorios saben que existe el outbox: siguen llamando a
/// SaveChangesAsync como antes.
/// </summary>
public sealed class OutboxInterceptor(IDomainEventMapper mapper, IOutboxSignal signal) : SaveChangesInterceptor
{
    // Hay una instancia por DbContext (se crea al configurarlo), así que este campo no se comparte.
    private bool _hasNewMessages;

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        AddOutboxMessages(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        AddOutboxMessages(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        NotifyIfNeeded();
        return base.SavedChanges(eventData, result);
    }

    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        NotifyIfNeeded();
        return base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        _hasNewMessages = false;
        base.SaveChangesFailed(eventData);
    }

    public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        _hasNewMessages = false;
        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    private void AddOutboxMessages(DbContext? context)
    {
        if (context is null) return;

        // Entries<IAggregateRoot>() = todas las entidades rastreadas que son aggregate roots,
        // sin importar su tipo concreto (para eso existe la interfaz no genérica IAggregateRoot).
        var aggregates = context.ChangeTracker
            .Entries<IAggregateRoot>()
            .Select(entry => entry.Entity)
            .Where(aggregate => aggregate.DomainEvents.Count > 0)
            .ToList();

        foreach (var aggregate in aggregates)
        {
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                if (mapper.Map(domainEvent) is { } integrationEvent)
                {
                    context.Set<OutboxMessage>().Add(OutboxSerializer.ToMessage(integrationEvent));
                    _hasNewMessages = true;
                }
            }

            // Ya están en el outbox: si se volviera a guardar, no se duplicarían.
            aggregate.ClearDomainEvents();
        }
    }

    private void NotifyIfNeeded()
    {
        if (!_hasNewMessages) return;

        _hasNewMessages = false;
        signal.Notify();
    }
}
