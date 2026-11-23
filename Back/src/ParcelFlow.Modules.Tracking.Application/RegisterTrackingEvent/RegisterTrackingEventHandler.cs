using ParcelFlow.BuildingBlocks.Application.Exceptions;
using ParcelFlow.BuildingBlocks.Application.Messaging;
using ParcelFlow.BuildingBlocks.Application.Parsing;
using ParcelFlow.Modules.Tracking.Application.Abstractions;
using ParcelFlow.Modules.Tracking.Domain;

namespace ParcelFlow.Modules.Tracking.Application.RegisterTrackingEvent;

/// <summary>
/// cargar → método del dominio → guardar. El dominio valida la transición (Created → InTransit
/// no se puede) y el orden de fechas; aquí solo se valida lo que es de la aplicación:
/// qué estados son manuales y que la fecha no esté en el futuro (el dominio no conoce "ahora").
/// </summary>
internal sealed class RegisterTrackingEventHandler(
    IShipmentTrackingRepository repository,
    ITrackingUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<RegisterTrackingEventCommand, Unit>
{
    /// <summary>Margen por relojes desajustados entre el navegador y el servidor.</summary>
    public static readonly TimeSpan AllowedClockSkew = TimeSpan.FromMinutes(5);

    public async Task<Unit> Handle(RegisterTrackingEventCommand command, CancellationToken cancellationToken)
    {
        var status = EnumParsing.Parse<TrackingStatus>(command.Status, nameof(command.Status));

        if (!ManualTrackingStatuses.IsManual(status))
            throw new ArgumentException(
                $"El estado {status} no se registra a mano: lo actualiza el módulo correspondiente.",
                nameof(command.Status));

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var occurredAt = command.OccurredAt?.ToUniversalTime() ?? now;

        if (occurredAt > now + AllowedClockSkew)
            throw new ArgumentException("La fecha del movimiento no puede estar en el futuro.", nameof(command.OccurredAt));

        var trackingNumber = TrackingNumberText.Normalize(command.TrackingNumber);

        var tracking = await repository.GetByTrackingNumberAsync(trackingNumber, cancellationToken)
            ?? throw new NotFoundException(
                "tracking.not_found",
                $"No hay seguimiento para la guía '{trackingNumber}'.");

        tracking.RegisterEvent(
            status,
            new Location(command.Location),
            new TrackingDescription(command.Description),
            occurredAt);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
