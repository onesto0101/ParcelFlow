using ParcelFlow.BuildingBlocks.Application.Messaging;
using ParcelFlow.Modules.Deliveries.Application.Abstractions;
using ParcelFlow.Modules.Deliveries.Domain;

namespace ParcelFlow.Modules.Deliveries.Application.GetDeliveryById;

internal sealed class GetDeliveryByIdHandler(IDeliveryRepository repository) : IQueryHandler<GetDeliveryByIdQuery, DeliveryDto>
{
    public async Task<DeliveryDto> Handle(GetDeliveryByIdQuery query, CancellationToken cancellationToken)
    {
        var delivery = await repository.GetByIdAsync(query.DeliveryId, cancellationToken)
            ?? throw DeliveryNotFound.For(query.DeliveryId);

        return new DeliveryDto(
            delivery.Id,
            delivery.ShipmentId,
            delivery.TrackingNumber,
            delivery.Status.ToString(),
            delivery.CourierId?.Value,
            delivery.CreatedAt,
            delivery.AssignedAt,
            delivery.CompletedAt,
            Delivery.MaxAttempts,
            delivery.RemainingAttempts,
            CanAssign: delivery.Status is DeliveryStatus.Pending or DeliveryStatus.Assigned,
            CanRegisterAttempt: delivery.Status == DeliveryStatus.Assigned,
            delivery.Attempts
                .OrderBy(a => a.AttemptNumber)
                .Select(a => new DeliveryAttemptDto(
                    a.AttemptNumber,
                    a.CourierId.Value,
                    a.AttemptedAt,
                    a.Succeeded,
                    a.Evidence?.ReceivedBy,
                    a.Evidence?.Notes,
                    a.FailureReason?.Code.ToString(),
                    a.FailureReason?.Details))
                .ToList());
    }
}
