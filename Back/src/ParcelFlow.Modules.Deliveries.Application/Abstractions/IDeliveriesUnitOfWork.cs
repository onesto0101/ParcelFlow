namespace ParcelFlow.Modules.Deliveries.Application.Abstractions;

/// <summary>Unit of work del módulo Deliveries.</summary>
public interface IDeliveriesUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
