using ParcelFlow.BuildingBlocks.Application.Messaging;

namespace ParcelFlow.Modules.Deliveries.Application.ReportDeliveryFailure;

/// <summary>DE-004, DE-006, DE-007: registrar un intento fallido con su motivo.</summary>
/// <param name="FailureCode">RecipientAbsent, WrongAddress, Refused, Inaccessible u Other.</param>
/// <param name="Details">Obligatorio si el motivo es Other (lo exige el dominio).</param>
public sealed record ReportDeliveryFailureCommand(Guid DeliveryId, string FailureCode, string? Details = null) : ICommand<Unit>;
