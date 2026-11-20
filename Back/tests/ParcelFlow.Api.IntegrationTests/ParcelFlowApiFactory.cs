using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace ParcelFlow.Api.IntegrationTests;

/// <summary>
/// Arranca la API COMPLETA en memoria (WebApplicationFactory) contra un Postgres real en Docker.
/// Es lo más parecido a producción que hay en un test: mismo Program.cs, mismo pipeline,
/// mismos módulos, misma base de datos. Solo cambia la cadena de conexión.
///
/// No abre ningún puerto: CreateClient() da un HttpClient que habla con la API en memoria.
/// </summary>
public sealed class ParcelFlowApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string FrontendOrigin = "http://localhost:5173";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18").Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Entorno Development → Program.cs aplica las migraciones al arrancar.
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:ParcelFlow", _postgres.GetConnectionString());
        builder.UseSetting("Cors:AllowedOrigins:0", FrontendOrigin);
    }

    // El contenedor tiene que estar arriba ANTES de que se cree la API (que migra al arrancar).
    public Task InitializeAsync() => _postgres.StartAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
