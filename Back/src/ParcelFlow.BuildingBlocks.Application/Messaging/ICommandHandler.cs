namespace ParcelFlow.BuildingBlocks.Application.Messaging;

/// <summary>
/// Atiende un comando concreto. Hay exactamente un handler por comando.
/// La restricción "where" obliga a que comando y handler coincidan en el tipo de resultado.
/// </summary>
/// <typeparam name="TCommand">El comando que atiende (ej. CreateShipmentCommand).</typeparam>
/// <typeparam name="TResult">Lo que devuelve (ej. el Guid del envío creado).</typeparam>
public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    Task<TResult> Handle(TCommand command, CancellationToken cancellationToken);
}
