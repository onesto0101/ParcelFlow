namespace ParcelFlow.Modules.Tracking.Application.Abstractions;

/// <summary>Igual que IShipmentsUnitOfWork, pero del módulo Tracking (cada módulo tiene el suyo).</summary>
public interface ITrackingUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
