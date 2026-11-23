using Microsoft.AspNetCore.Http.HttpResults;
using ParcelFlow.BuildingBlocks.Application.Messaging;
using ParcelFlow.BuildingBlocks.Application.Paging;
using ParcelFlow.Modules.Deliveries.Application.AssignCourier;
using ParcelFlow.Modules.Deliveries.Application.ConfirmDelivery;
using ParcelFlow.Modules.Deliveries.Application.GetDeliveries;
using ParcelFlow.Modules.Deliveries.Application.GetDeliveryById;
using ParcelFlow.Modules.Deliveries.Application.ReportDeliveryFailure;

namespace ParcelFlow.Api.Endpoints;

/// <summary>
/// Endpoints del módulo Deliveries. Las acciones son POST a sub-rutas con un verbo
/// (/assign, /confirm, /fail), igual que /cancel en Shipments: son transiciones de estado,
/// no "editar campos" de la entrega.
/// </summary>
public static class DeliveryEndpoints
{
    public static IEndpointRouteBuilder MapDeliveryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/deliveries").WithTags("Deliveries");

        group.MapGet("/", GetDeliveries)
            .WithName("GetDeliveries")
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:guid}", GetDelivery)
            .WithName("GetDelivery")
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/assign", AssignCourier)
            .WithName("AssignCourier")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/confirm", ConfirmDelivery)
            .WithName("ConfirmDelivery")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/fail", ReportFailure)
            .WithName("ReportDeliveryFailure")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }

    // GET /api/deliveries?status=Pending&courierId=REP-014&search=PF7K&page=1
    private static async Task<Ok<PagedResult<DeliverySummaryDto>>> GetDeliveries(
        [AsParameters] GetDeliveriesQuery query, IMediator mediator, CancellationToken cancellationToken) =>
        TypedResults.Ok(await mediator.Query(query, cancellationToken));

    private static async Task<Ok<DeliveryDto>> GetDelivery(Guid id, IMediator mediator, CancellationToken cancellationToken) =>
        TypedResults.Ok(await mediator.Query(new GetDeliveryByIdQuery(id), cancellationToken));

    private static async Task<NoContent> AssignCourier(
        Guid id, AssignCourierRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        await mediator.Send(new AssignCourierCommand(id, request.CourierId), cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> ConfirmDelivery(
        Guid id, ConfirmDeliveryRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        await mediator.Send(new ConfirmDeliveryCommand(id, request.ReceivedBy, request.Notes), cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> ReportFailure(
        Guid id, ReportDeliveryFailureRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        await mediator.Send(new ReportDeliveryFailureCommand(id, request.FailureCode, request.Details), cancellationToken);
        return TypedResults.NoContent();
    }
}

public sealed record AssignCourierRequest(string CourierId);

public sealed record ConfirmDeliveryRequest(string ReceivedBy, string? Notes);

public sealed record ReportDeliveryFailureRequest(string FailureCode, string? Details);
