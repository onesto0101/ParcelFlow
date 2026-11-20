using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ParcelFlow.Api.IntegrationTests;

/// <summary>
/// Tests "de punta a punta": HTTP → endpoint → mediator → handler → dominio → EF Core → Postgres.
/// Se escriben desde el punto de vista del front: solo JSON y códigos HTTP,
/// sin usar ningún tipo interno de la aplicación.
/// </summary>
public class ShipmentEndpointsTests(ParcelFlowApiFactory factory) : IClassFixture<ParcelFlowApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static object NewShipmentRequest(decimal weightKg = 5m) => new
    {
        sender = new
        {
            name = "Ana López",
            phone = "81 1234 5678",
            address = new { street = "Av. Constitución 100", city = "Monterrey", state = "Nuevo León", postalCode = "64000", country = "MX" },
        },
        recipient = new
        {
            name = "Luis Pérez",
            phone = "+52 33 9876 5432",
            address = new { street = "Calle Juárez 5", city = "Guadalajara", state = "Jalisco", postalCode = "44100", country = "MX" },
        },
        weightKg,
        lengthCm = 30m,
        widthCm = 20m,
        heightCm = 10m,
    };

    private async Task<JsonElement> CreateShipmentAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/shipments", NewShipmentRequest());
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static async Task<string?> ProblemCodeAsync(HttpResponseMessage response)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        return problem.GetProperty("code").GetString();
    }

    [Fact]
    public async Task Create_returns_201_with_location_and_the_shipment_can_be_read_back()
    {
        var response = await _client.PostAsJsonAsync("/api/shipments", NewShipmentRequest());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(125m, created.GetProperty("cost").GetDecimal());

        // La cabecera Location apunta a la URL de consulta del envío recién creado.
        var get = await _client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);

        var shipment = await get.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(created.GetProperty("trackingNumber").GetString(), shipment.GetProperty("trackingNumber").GetString());
        Assert.Equal("Registered", shipment.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Invalid_weight_returns_400_problem_with_field()
    {
        var response = await _client.PostAsJsonAsync("/api/shipments", NewShipmentRequest(weightKg: 80m));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("validation.invalid", problem.GetProperty("code").GetString());
        Assert.Equal("Kilograms", problem.GetProperty("field").GetString());
    }

    [Fact]
    public async Task Malformed_json_returns_400()
    {
        var response = await _client.PostAsync(
            "/api/shipments",
            new StringContent("{ \"sender\": ", System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("request.invalid", await ProblemCodeAsync(response));
    }

    [Fact]
    public async Task Unknown_tracking_number_returns_404()
    {
        var response = await _client.GetAsync("/api/shipments/tracking/PF2222222222");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("shipment.not_found", await ProblemCodeAsync(response));
    }

    [Fact]
    public async Task Cancel_returns_204_and_cancelling_again_returns_409()
    {
        var created = await CreateShipmentAsync();
        var url = $"/api/shipments/{created.GetProperty("shipmentId").GetGuid()}/cancel";

        var first = await _client.PostAsJsonAsync(url, new { reason = "El cliente ya no lo necesita" });
        var second = await _client.PostAsJsonAsync(url, new { reason = "otra vez" });

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        Assert.Equal("shipment.already_cancelled", await ProblemCodeAsync(second));
    }

    [Fact]
    public async Task Cors_preflight_allows_the_frontend_origin_only()
    {
        static HttpRequestMessage Preflight(string origin)
        {
            var request = new HttpRequestMessage(HttpMethod.Options, "/api/shipments");
            request.Headers.Add("Origin", origin);
            request.Headers.Add("Access-Control-Request-Method", "POST");
            return request;
        }

        var allowed = await _client.SendAsync(Preflight(ParcelFlowApiFactory.FrontendOrigin));
        var other = await _client.SendAsync(Preflight("http://sitio-malicioso.example"));

        Assert.Equal(
            ParcelFlowApiFactory.FrontendOrigin,
            allowed.Headers.GetValues("Access-Control-Allow-Origin").Single());
        Assert.False(other.Headers.Contains("Access-Control-Allow-Origin"));
    }
}
