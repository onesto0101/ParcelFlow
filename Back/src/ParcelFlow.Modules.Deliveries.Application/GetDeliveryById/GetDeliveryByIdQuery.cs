using ParcelFlow.BuildingBlocks.Application.Messaging;

namespace ParcelFlow.Modules.Deliveries.Application.GetDeliveryById;

/// <summary>Detalle de una entrega con su historial de intentos (DE-009).</summary>
public sealed record GetDeliveryByIdQuery(Guid DeliveryId) : IQuery<DeliveryDto>;

/// <param name="CanAssign">Si se puede asignar/reasignar repartidor ahora.</param>
/// <param name="CanRegisterAttempt">Si se puede confirmar o reportar un fallo ahora.</param>
/// <remarks>
/// CanAssign y CanRegisterAttempt los calcula el backend con las reglas del dominio: el front solo
/// decide si muestra o no los botones, sin copiar las reglas en TypeScript.
/// </remarks>
public sealed record DeliveryDto(
    Guid Id,
    Guid ShipmentId,
    string TrackingNumber,
    string Status,
    string? CourierId,
    DateTime CreatedAt,
    DateTime? AssignedAt,
    DateTime? CompletedAt,
    int MaxAttempts,
    int RemainingAttempts,
    bool CanAssign,
    bool CanRegisterAttempt,
    IReadOnlyList<DeliveryAttemptDto> Attempts);

public sealed record DeliveryAttemptDto(
    int AttemptNumber,
    string CourierId,
    DateTime AttemptedAt,
    bool Succeeded,
    string? ReceivedBy,
    string? Notes,
    string? FailureCode,
    string? FailureDetails);
