namespace ParcelFlow.BuildingBlocks.Application.Messaging;

/// <summary>
/// Marca un mensaje como CONSULTA: pide datos y NO cambia nada.
/// Se puede repetir cuantas veces se quiera sin efectos secundarios.
/// Su nombre empieza con Get: GetShipmentByTrackingNumber.
/// </summary>
/// <typeparam name="TResult">Lo que devuelve (normalmente un DTO, no la entidad del dominio).</typeparam>
public interface IQuery<out TResult>;
