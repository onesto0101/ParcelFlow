namespace ParcelFlow.BuildingBlocks.Application.Messaging;

/// <summary>
/// Marca un mensaje como COMANDO: una intención de cambiar el estado del sistema
/// (crear, cancelar, marcar como entregado...). Su nombre va en imperativo: CreateShipment.
///
/// No tiene miembros: solo sirve para "etiquetar" el tipo y declarar qué devuelve.
/// Si un comando no devuelve nada útil, usa <see cref="Unit"/> como TResult.
/// </summary>
/// <typeparam name="TResult">Lo que devuelve al ejecutarse (ej. el Id creado).</typeparam>
public interface ICommand<out TResult>;
