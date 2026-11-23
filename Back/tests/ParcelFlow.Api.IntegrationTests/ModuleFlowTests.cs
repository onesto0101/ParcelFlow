using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ParcelFlow.Api.IntegrationTests;

/// <summary>
/// El sistema completo: los tres módulos hablando entre sí por integration events a través del
/// outbox real (procesadores en segundo plano incluidos) y un Postgres real.
///
/// Es la prueba de que "Shipments → Tracking → Deliveries → Tracking + Shipments" funciona
/// sin que ningún módulo llame a otro directamente.
/// </summary>
public class ModuleFlowTests(ParcelFlowApiFactory factory) : IClassFixture<ParcelFlowApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static string Status(JsonElement json, string property = "currentStatus") => json.GetProperty(property).GetString()!;

    private Task RegisterMovement(string trackingNumber, string status, string location) =>
        _client.PostOkAsync($"/api/tracking/{trackingNumber}/events", new { status, location, description = $"Movimiento {status}" });

    /// <summary>Crea un envío y lo lleva hasta el centro de distribución; devuelve el Id de su entrega.</summary>
    private async Task<(Guid ShipmentId, string TrackingNumber, Guid DeliveryId)> ShipmentReadyForDeliveryAsync()
    {
        var (shipmentId, tn) = await _client.CreateShipmentAsync();

        await _client.EventuallyAsync($"/api/tracking/{tn}", j => Status(j) == "Created", "Tracking inicia el seguimiento");

        await RegisterMovement(tn, "PickedUp", "Monterrey");
        await RegisterMovement(tn, "InTransit", "Carretera 57");
        await RegisterMovement(tn, "AtDistributionCenter", "CEDIS Guadalajara");

        var deliveries = await _client.EventuallyAsync(
            $"/api/deliveries?search={tn}", j => j.GetProperty("totalCount").GetInt32() == 1, "Deliveries crea la entrega pendiente");

        var delivery = deliveries.GetProperty("items")[0];
        Assert.Equal("Pending", delivery.GetProperty("status").GetString());

        return (shipmentId, tn, delivery.GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task Full_lifecycle_with_one_failed_attempt()
    {
        var (_, tn, deliveryId) = await ShipmentReadyForDeliveryAsync();
        var trackingUrl = $"/api/tracking/{tn}";

        // 1er intento: falla.
        await _client.PostOkAsync($"/api/deliveries/{deliveryId}/assign", new { courierId = "REP-014" });
        await _client.EventuallyAsync(trackingUrl, j => Status(j) == "OutForDelivery", "Tracking → En reparto");

        await _client.PostOkAsync($"/api/deliveries/{deliveryId}/fail", new { failureCode = "RecipientAbsent" });
        await _client.EventuallyAsync(trackingUrl, j => Status(j) == "DeliveryFailed", "Tracking → Entrega fallida");

        var afterFailure = await _client.GetJsonAsync($"/api/deliveries/{deliveryId}");
        Assert.Equal("Pending", afterFailure.GetProperty("status").GetString());
        Assert.Equal(2, afterFailure.GetProperty("remainingAttempts").GetInt32());

        // 2º intento: éxito.
        await _client.PostOkAsync($"/api/deliveries/{deliveryId}/assign", new { courierId = "REP-020" });
        await _client.EventuallyAsync(trackingUrl, j => Status(j) == "OutForDelivery", "Tracking → En reparto otra vez");
        await _client.PostOkAsync($"/api/deliveries/{deliveryId}/confirm", new { receivedBy = "Luis Pérez", notes = "Recepción del edificio" });

        // Un solo comando en Deliveries actualiza los otros dos módulos.
        var tracking = await _client.EventuallyAsync(trackingUrl, j => Status(j) == "Delivered", "Tracking → Entregado");
        await _client.EventuallyAsync($"/api/shipments/tracking/{tn}", j => Status(j, "status") == "Delivered", "Shipments → Delivered");

        Assert.True(tracking.GetProperty("isFinal").GetBoolean());
        Assert.Empty(tracking.GetProperty("nextManualStatuses").EnumerateArray());
        string[] expectedHistory = ["Created", "PickedUp", "InTransit", "AtDistributionCenter", "OutForDelivery", "DeliveryFailed", "OutForDelivery", "Delivered"];
        Assert.Equal(expectedHistory, tracking.GetProperty("events").EnumerateArray().Select(e => e.GetProperty("status").GetString()));

        var delivery = await _client.GetJsonAsync($"/api/deliveries/{deliveryId}");
        Assert.Equal("Delivered", delivery.GetProperty("status").GetString());
        var attempts = delivery.GetProperty("attempts").EnumerateArray().ToList();
        Assert.Equal(2, attempts.Count);
        Assert.Equal("RecipientAbsent", attempts[0].GetProperty("failureCode").GetString());
        Assert.Equal("Luis Pérez", attempts[1].GetProperty("receivedBy").GetString());
    }

    [Fact]
    public async Task Cancelling_the_shipment_cancels_tracking_and_delivery()
    {
        var (shipmentId, tn, deliveryId) = await ShipmentReadyForDeliveryAsync();

        await _client.PostOkAsync($"/api/shipments/{shipmentId}/cancel", new { reason = "El cliente ya no lo necesita" });

        await _client.EventuallyAsync($"/api/tracking/{tn}", j => Status(j) == "Cancelled", "Tracking → Cancelado");
        await _client.EventuallyAsync($"/api/deliveries/{deliveryId}", j => Status(j, "status") == "Cancelled", "Deliveries → Cancelada");
    }

    [Fact]
    public async Task Operator_cannot_register_statuses_owned_by_other_modules_or_skip_steps()
    {
        var (_, tn) = await _client.CreateShipmentAsync();
        await _client.EventuallyAsync($"/api/tracking/{tn}", j => Status(j) == "Created", "Tracking inicia el seguimiento");

        var manualDelivered = await _client.PostAsJsonAsync($"/api/tracking/{tn}/events",
            new { status = "Delivered", location = "Guadalajara", description = "Entregado" });
        var skipped = await _client.PostAsJsonAsync($"/api/tracking/{tn}/events",
            new { status = "InTransit", location = "Guadalajara", description = "Salto" });

        Assert.Equal(HttpStatusCode.BadRequest, manualDelivered.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, skipped.StatusCode);
    }

    [Fact]
    public async Task Confirming_a_pending_delivery_returns_409()
    {
        var (_, _, deliveryId) = await ShipmentReadyForDeliveryAsync();

        var response = await _client.PostAsJsonAsync($"/api/deliveries/{deliveryId}/confirm", new { receivedBy = "Luis Pérez" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("delivery.not_assigned", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Unknown_tracking_returns_404()
    {
        var response = await _client.GetAsync("/api/tracking/PF2222222222");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Dashboard_counts_what_each_module_knows()
    {
        var (_, tn) = await _client.CreateShipmentAsync();
        await _client.EventuallyAsync($"/api/tracking/{tn}", j => Status(j) == "Created", "Tracking inicia el seguimiento");
        await RegisterMovement(tn, "PickedUp", "Monterrey");

        var dashboard = await _client.GetJsonAsync("/api/dashboard");

        Assert.True(dashboard.GetProperty("totalShipments").GetInt32() >= 1);
        Assert.True(dashboard.GetProperty("inTransit").GetInt32() >= 1);
        Assert.True(dashboard.GetProperty("tracking").GetProperty("pickedUp").GetInt32() >= 1);
        Assert.True(dashboard.TryGetProperty("deliveries", out _));
    }
}
