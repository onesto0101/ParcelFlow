using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ParcelFlow.Api.IntegrationTests;

internal static class ApiTestHelpers
{
    public static object ShipmentRequest(string recipientName = "Luis Pérez", string originCity = "Monterrey") => new
    {
        sender = new
        {
            name = "Ana López",
            phone = "81 1234 5678",
            address = new { street = "Av. Constitución 100", city = originCity, state = "Nuevo León", postalCode = "64000", country = "MX" },
        },
        recipient = new
        {
            name = recipientName,
            phone = "+52 33 9876 5432",
            address = new { street = "Calle Juárez 5", city = "Guadalajara", state = "Jalisco", postalCode = "44100", country = "MX" },
        },
        weightKg = 5m,
        lengthCm = 30m,
        widthCm = 20m,
        heightCm = 10m,
    };

    public static async Task<(Guid Id, string TrackingNumber)> CreateShipmentAsync(this HttpClient client, string recipientName = "Luis Pérez")
    {
        var response = await client.PostAsJsonAsync("/api/shipments", ShipmentRequest(recipientName));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return (body.GetProperty("shipmentId").GetGuid(), body.GetProperty("trackingNumber").GetString()!);
    }

    public static async Task<JsonElement> GetJsonAsync(this HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    public static async Task PostOkAsync(this HttpClient client, string url, object body)
    {
        var response = await client.PostAsJsonAsync(url, body);
        Assert.True(response.StatusCode == HttpStatusCode.NoContent,
            $"POST {url} → {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
    }

    /// <summary>
    /// Consistencia EVENTUAL: lo que pasa en otro módulo ocurre "un poco después" (cuando el outbox
    /// publica). El test no puede comprobarlo justo tras el POST: reintenta hasta que se cumpla
    /// o hasta el timeout. Nunca uses Thread.Sleep(fijo): o es lento o falla a veces.
    /// </summary>
    public static async Task<JsonElement> EventuallyAsync(
        this HttpClient client, string url, Func<JsonElement, bool> condition, string description)
    {
        var deadline = DateTime.UtcNow.AddSeconds(15);
        string last = "(sin respuesta)";

        while (DateTime.UtcNow < deadline)
        {
            var response = await client.GetAsync(url);
            last = $"{(int)response.StatusCode} {await response.Content.ReadAsStringAsync()}";

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var json = await response.Content.ReadFromJsonAsync<JsonElement>();
                if (condition(json)) return json;
            }

            await Task.Delay(100);
        }

        throw new Xunit.Sdk.XunitException($"No se cumplió a tiempo: {description}. Última respuesta de {url}: {last}");
    }
}
