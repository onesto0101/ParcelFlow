using Microsoft.AspNetCore.Http.HttpResults;
using ParcelFlow.BuildingBlocks.Application.Messaging;
using ParcelFlow.BuildingBlocks.Application.Paging;
using ParcelFlow.Modules.Shipments.Application.CancelShipment;
using ParcelFlow.Modules.Shipments.Application.CreateShipment;
using ParcelFlow.Modules.Shipments.Application.GetShipmentById;
using ParcelFlow.Modules.Shipments.Application.GetShipmentByTrackingNumber;
using ParcelFlow.Modules.Shipments.Application.GetShipments;

namespace ParcelFlow.Api.Endpoints;

/// <summary>
/// Endpoints HTTP del módulo Shipments (Minimal APIs).
///
/// Cada endpoint hace solo 3 cosas: leer la petición → mandar el mensaje al mediator → elegir
/// el código HTTP de éxito. No hay lógica de negocio ni try/catch (los errores los traduce
/// ExceptionToProblemDetailsHandler).
/// </summary>
public static class ShipmentEndpoints
{
    public static IEndpointRouteBuilder MapShipmentEndpoints(this IEndpointRouteBuilder app)
    {
        // Un grupo comparte prefijo de ruta y etiqueta en OpenAPI.
        var group = app.MapGroup("/api/shipments").WithTags("Shipments");

        group.MapGet("/", GetShipments)
            .WithName("GetShipments")
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/", CreateShipment)
            .WithName("CreateShipment")
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/tracking/{trackingNumber}", GetShipmentByTrackingNumber)
            .WithName("GetShipmentByTrackingNumber")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}", GetShipmentById)
            .WithName("GetShipmentById")
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/cancel", CancelShipment)
            .WithName("CancelShipment")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }

    // GET /api/shipments?status=Registered&search=monterrey&page=1&pageSize=20  → 200 OK + página.
    // [AsParameters] junta los parámetros de la query string en el record (mismos nombres).
    private static async Task<Ok<PagedResult<ShipmentSummaryDto>>> GetShipments(
        [AsParameters] GetShipmentsQuery query,
        IMediator mediator,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await mediator.Query(query, cancellationToken));

    // POST /api/shipments  → 201 Created + cabecera Location con la URL para consultarlo.
    // El cuerpo JSON se convierte directamente en el comando (mismos campos).
    private static async Task<Created<CreateShipmentResult>> CreateShipment(
        CreateShipmentCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        return TypedResults.Created($"/api/shipments/tracking/{result.TrackingNumber}", result);
    }

    // GET /api/shipments/tracking/PF7K3M9QXR2A  → 200 OK + ShipmentDto.
    private static async Task<Ok<ShipmentDto>> GetShipmentByTrackingNumber(
        string trackingNumber,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var shipment = await mediator.Query(new GetShipmentByTrackingNumberQuery(trackingNumber), cancellationToken);

        return TypedResults.Ok(shipment);
    }

    // GET /api/shipments/{id:guid}  → 200 OK + ShipmentDto.
    private static async Task<Ok<ShipmentDto>> GetShipmentById(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var shipment = await mediator.Query(new GetShipmentByIdQuery(id), cancellationToken);

        return TypedResults.Ok(shipment);
    }

    // POST /api/shipments/{id}/cancel  { "reason": "..." }  → 204 No Content.
    // Es POST (una acción) y no DELETE: el envío no se borra, cambia de estado y conserva su historial.
    private static async Task<NoContent> CancelShipment(
        Guid id,
        CancelShipmentRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new CancelShipmentCommand(id, request.Reason), cancellationToken);

        return TypedResults.NoContent();
    }
}

/// <summary>
/// Cuerpo de la petición de cancelación. No es el comando: el Id viene en la URL,
/// así que el cuerpo solo trae lo que falta.
/// </summary>
public sealed record CancelShipmentRequest(string Reason);
