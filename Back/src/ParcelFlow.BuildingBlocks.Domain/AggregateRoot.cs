namespace ParcelFlow.BuildingBlocks.Domain;

/// <summary>
/// Vista no genérica de un aggregate root. Sirve para que la infraestructura
/// (por ejemplo, el DbContext) pueda recoger los eventos de cualquier aggregate
/// sin conocer su tipo concreto.
/// </summary>
public interface IAggregateRoot
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}

/// <summary>
/// Raíz de un aggregate: la única puerta de entrada para modificarlo.
/// Registra los eventos de dominio que ocurren mientras cambia su estado.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
