namespace ParcelFlow.Modules.Shipments.Application.Abstractions;

/// <summary>
/// Confirma (guarda) de una sola vez todos los cambios hechos durante el caso de uso.
/// O se guarda todo o no se guarda nada (una transacción).
///
/// Es específico del módulo (IShipmentsUnitOfWork y no un IUnitOfWork genérico) porque cada módulo
/// tendrá su propio DbContext: si los tres registraran "IUnitOfWork", el contenedor no sabría cuál dar.
/// </summary>
public interface IShipmentsUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
