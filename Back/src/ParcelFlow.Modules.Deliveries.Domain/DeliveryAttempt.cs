using ParcelFlow.BuildingBlocks.Domain;

namespace ParcelFlow.Modules.Deliveries.Domain;

/// <summary>
/// Un intento de entrega. Es una entidad inmutable: un intento ya realizado no se modifica.
///
/// Un intento es exitoso (tiene <see cref="Evidence"/>) o fallido (tiene
/// <see cref="FailureReason"/>), nunca las dos cosas. Los dos métodos de fábrica
/// (<see cref="Successful"/> y <see cref="Failed"/>) lo garantizan: no hay forma de
/// construir un intento "a medias".
/// </summary>
public sealed class DeliveryAttempt : Entity<Guid>
{
    public int AttemptNumber { get; private set; }
    public CourierId CourierId { get; private set; } = null!;
    public DateTime AttemptedAt { get; private set; }
    public bool Succeeded { get; private set; }
    public DeliveryEvidence? Evidence { get; private set; }
    public FailureReason? FailureReason { get; private set; }

    private DeliveryAttempt() { }

    internal static DeliveryAttempt Successful(int attemptNumber, CourierId courierId, DeliveryEvidence evidence, DateTime attemptedAt) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            AttemptNumber = attemptNumber,
            CourierId = courierId,
            AttemptedAt = attemptedAt,
            Succeeded = true,
            Evidence = evidence,
        };

    internal static DeliveryAttempt Failed(int attemptNumber, CourierId courierId, FailureReason reason, DateTime attemptedAt) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            AttemptNumber = attemptNumber,
            CourierId = courierId,
            AttemptedAt = attemptedAt,
            Succeeded = false,
            FailureReason = reason,
        };
}
