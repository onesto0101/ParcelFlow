using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ParcelFlow.BuildingBlocks.Application.Messaging;

/// <summary>
/// Implementación del mediator: busca en el contenedor de dependencias (DI) el handler
/// que corresponde al mensaje y lo ejecuta.
///
/// Problema que resuelve: en Send&lt;TResult&gt; solo conocemos TResult en tiempo de compilación.
/// El tipo concreto del comando (CreateShipmentCommand) lo conocemos hasta que el programa corre.
/// Por eso armamos el tipo del handler con reflexión (MakeGenericType).
/// </summary>
public sealed class Mediator(IServiceProvider serviceProvider) : IMediator
{
    public Task<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        // command.GetType() = CreateShipmentCommand  →  ICommandHandler<CreateShipmentCommand, Guid>
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResult));

        return Invoke<TResult>(handlerType, command, cancellationToken);
    }

    public Task<TResult> Query<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));

        return Invoke<TResult>(handlerType, query, cancellationToken);
    }

    private Task<TResult> Invoke<TResult>(Type handlerType, object message, CancellationToken cancellationToken)
    {
        // Si no hay handler registrado, falla con un mensaje claro (InvalidOperationException).
        var handler = serviceProvider.GetRequiredService(handlerType);

        var handle = handlerType.GetMethod("Handle")!;

        // DoNotWrapExceptions: si el handler lanza, recibimos SU excepción (ej. DomainException)
        // y no una TargetInvocationException que la envuelva. Importante para mapear errores a HTTP.
        return (Task<TResult>)handle.Invoke(
            handler,
            BindingFlags.DoNotWrapExceptions,
            binder: null,
            parameters: [message, cancellationToken],
            culture: null)!;
    }
}
