namespace ParcelFlow.BuildingBlocks.Application.Messaging;

/// <summary>
/// Atiende una consulta concreta. Hay exactamente un handler por consulta.
/// Regla: un query handler nunca modifica datos.
/// </summary>
/// <typeparam name="TQuery">La consulta que atiende (ej. GetShipmentByTrackingNumberQuery).</typeparam>
/// <typeparam name="TResult">Lo que devuelve (ej. un ShipmentDto).</typeparam>
public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    Task<TResult> Handle(TQuery query, CancellationToken cancellationToken);
}
