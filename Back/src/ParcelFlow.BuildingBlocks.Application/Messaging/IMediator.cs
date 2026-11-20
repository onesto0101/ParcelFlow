namespace ParcelFlow.BuildingBlocks.Application.Messaging;

/// <summary>
/// Punto de entrada único para ejecutar casos de uso.
/// Quien llama (la API) solo conoce el mensaje, no el handler que lo atiende.
///
/// Solo hay un parámetro genérico (TResult) y el compilador lo deduce del mensaje:
///   Guid id = await mediator.Send(new CreateShipmentCommand(...), ct);
/// </summary>
public interface IMediator
{
    Task<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);

    Task<TResult> Query<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}
