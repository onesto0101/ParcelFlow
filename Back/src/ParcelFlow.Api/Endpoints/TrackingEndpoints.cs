using Microsoft.AspNetCore.Http.HttpResults;
using ParcelFlow.BuildingBlocks.Application.Messaging;
using ParcelFlow.Modules.Tracking.Application.GetTracking;
using ParcelFlow.Modules.Tracking.Application.RegisterTrackingEvent;

namespace ParcelFlow.Api.Endpoints;

/// <summary>Endpoints del módulo Tracking. Misma receta que ShipmentEndpoints.</summary>
public static class TrackingEndpoints
{
    public static IEndpointRouteBuilder MapTrackingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tracking").WithTags("Tracking");

        group.MapGet("/{trackingNumber}", GetTracking)
            .WithName("GetTracking")
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{trackingNumber}/events", RegisterEvent)
            .WithName("RegisterTrackingEvent")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }

    // GET /api/tracking/PF7K3M9QXR2A  → estado actual + historial.
    private static async Task<Ok<TrackingDto>> GetTracking(
        string trackingNumber, IMediator mediator, CancellationToken cancellationToken) =>
        TypedResults.Ok(await mediator.Query(new GetTrackingByTrackingNumberQuery(trackingNumber), cancellationToken));

    // POST /api/tracking/PF7K3M9QXR2A/events  { "status": "PickedUp", "location": "...", "description": "..." }
    // → 204. Un movimiento es un recurso hijo del seguimiento, de ahí la URL ".../events".
    private static async Task<NoContent> RegisterEvent(
        string trackingNumber,
        RegisterTrackingEventRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        await mediator.Send(
            new RegisterTrackingEventCommand(trackingNumber, request.Status, request.Location, request.Description, request.OccurredAt),
            cancellationToken);

        return TypedResults.NoContent();
    }
}

public sealed record RegisterTrackingEventRequest(string Status, string Location, string Description, DateTime? OccurredAt);
