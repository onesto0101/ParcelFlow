using System.Net;
using System.Text.Json;

namespace ParcelFlow.Api.IntegrationTests;

/// <summary>
/// GET /api/shipments: filtros, búsqueda y paginación contra Postgres real.
/// La BD se comparte entre los tests de la clase, así que cada test usa un nombre único
/// para buscar SOLO sus envíos.
/// </summary>
public class ShipmentListTests(ParcelFlowApiFactory factory) : IClassFixture<ParcelFlowApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static string UniqueName() => $"Cliente {Guid.NewGuid():N}"[..20];

    private static int TotalCount(JsonElement page) => page.GetProperty("totalCount").GetInt32();

    [Fact]
    public async Task Search_by_recipient_name_ignoring_case_with_pagination()
    {
        var name = UniqueName();
        await _client.CreateShipmentAsync(name);
        await _client.CreateShipmentAsync(name);
        await _client.CreateShipmentAsync(name);

        var page1 = await _client.GetJsonAsync($"/api/shipments?search={name.ToUpperInvariant()}&pageSize=2");
        var page2 = await _client.GetJsonAsync($"/api/shipments?search={name}&pageSize=2&page=2");

        Assert.Equal(3, TotalCount(page1));
        Assert.Equal(2, page1.GetProperty("totalPages").GetInt32());
        Assert.Equal(2, page1.GetProperty("items").GetArrayLength());
        Assert.Single(page2.GetProperty("items").EnumerateArray());

        var row = page1.GetProperty("items")[0];
        Assert.Equal(name, row.GetProperty("recipientName").GetString());
        Assert.Equal("Monterrey", row.GetProperty("originCity").GetString());
    }

    [Fact]
    public async Task Search_by_part_of_the_tracking_number()
    {
        var (_, tn) = await _client.CreateShipmentAsync();

        var page = await _client.GetJsonAsync($"/api/shipments?search={tn[2..].ToLowerInvariant()}");

        Assert.Contains(page.GetProperty("items").EnumerateArray(), r => r.GetProperty("trackingNumber").GetString() == tn);
    }

    [Fact]
    public async Task Filter_by_status()
    {
        var name = UniqueName();
        var (cancelledId, _) = await _client.CreateShipmentAsync(name);
        await _client.CreateShipmentAsync(name);
        await _client.PostOkAsync($"/api/shipments/{cancelledId}/cancel", new { reason = "Prueba" });

        var cancelled = await _client.GetJsonAsync($"/api/shipments?search={name}&status=Cancelled");
        var registered = await _client.GetJsonAsync($"/api/shipments?search={name}&status=registered");

        Assert.Equal(1, TotalCount(cancelled));
        Assert.Equal(cancelledId, cancelled.GetProperty("items")[0].GetProperty("id").GetGuid());
        Assert.Equal(1, TotalCount(registered));
    }

    [Fact]
    public async Task Like_wildcards_in_the_search_are_treated_as_text()
    {
        var page = await _client.GetJsonAsync("/api/shipments?search=%25");

        Assert.Equal(0, TotalCount(page)); // "%" literal: ningún nombre lo contiene
    }

    [Fact]
    public async Task Invalid_status_returns_400()
    {
        var response = await _client.GetAsync("/api/shipments?status=Lost");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
