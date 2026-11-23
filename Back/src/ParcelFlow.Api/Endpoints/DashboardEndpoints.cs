using Microsoft.AspNetCore.Http.HttpResults;
using ParcelFlow.BuildingBlocks.Application.Messaging;
using ParcelFlow.Modules.Deliveries.Application.GetDeliveryStats;
using ParcelFlow.Modules.Shipments.Application.GetShipmentStats;
using ParcelFlow.Modules.Tracking.Application.GetTrackingStats;

namespace ParcelFlow.Api.Endpoints;

/// <summary>
/// Dashboard = "API composition": pide a CADA módulo sus números (tres queries independientes)
/// y los combina aquí, en la API, que es la única pieza que conoce a los tres.
///
/// Alternativa descartada: una query SQL que haga JOIN entre schemas (shipments + tracking +
/// deliveries). Sería más rápida, pero rompería las fronteras de los módulos: si mañana Tracking
/// cambia sus tablas, se rompería el Dashboard sin que Tracking lo sepa.
/// </summary>
public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/dashboard", GetDashboard)
            .WithTags("Dashboard")
            .WithName("GetDashboard");

        return app;
    }

    private static async Task<Ok<DashboardDto>> GetDashboard(IMediator mediator, CancellationToken cancellationToken)
    {
        // Secuenciales y no en paralelo (Task.WhenAll): comparten el scope de la petición y
        // un DbContext no admite dos operaciones a la vez. Son tres consultas COUNT muy baratas.
        var shipments = await mediator.Query(new GetShipmentStatsQuery(), cancellationToken);
        var tracking = await mediator.Query(new GetTrackingStatsQuery(), cancellationToken);
        var deliveries = await mediator.Query(new GetDeliveryStatsQuery(), cancellationToken);

        // Los 5 indicadores del documento de diseño (§8.2 Dashboard).
        return TypedResults.Ok(new DashboardDto(
            TotalShipments: shipments.Total,
            InTransit: tracking.PickedUp + tracking.InTransit + tracking.AtDistributionCenter,
            OutForDelivery: tracking.OutForDelivery,
            Delivered: shipments.Delivered,
            WithDeliveryProblems: tracking.DeliveryFailed,
            Shipments: shipments,
            Tracking: tracking,
            Deliveries: deliveries));
    }
}

/// <summary>Los indicadores + el desglose de cada módulo, por si el front quiere más detalle.</summary>
public sealed record DashboardDto(
    int TotalShipments,
    int InTransit,
    int OutForDelivery,
    int Delivered,
    int WithDeliveryProblems,
    ShipmentStatsDto Shipments,
    TrackingStatsDto Tracking,
    DeliveryStatsDto Deliveries);
