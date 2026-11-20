using ParcelFlow.Api.Endpoints;
using ParcelFlow.Api.ErrorHandling;
using ParcelFlow.Modules.Shipments.Application;
using ParcelFlow.Modules.Shipments.Infrastructure;

// ───────────────────────── 1. Registro de servicios (qué existe) ─────────────────────────
var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("ParcelFlow")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'ConnectionStrings:ParcelFlow'.");

// El reloj real. Los handlers piden TimeProvider; en los tests se cambia por FakeTimeProvider.
builder.Services.AddSingleton(TimeProvider.System);

// Módulos: la API no conoce handlers, repositorios ni DbContext; solo "enchufa" cada módulo.
builder.Services
    .AddShipmentsApplication()
    .AddShipmentsInfrastructure(connectionString);

// Errores → respuestas ProblemDetails (RFC 9457), el formato estándar de error en HTTP.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ExceptionToProblemDetailsHandler>();

// Documento OpenAPI (/openapi/v1.json): describe la API; el front podrá generar tipos a partir de él.
builder.Services.AddOpenApi();

// Endpoint /health: el hosting (Render) lo consulta para saber si la API está viva.
builder.Services.AddHealthChecks();

// CORS: el navegador bloquea llamadas desde otro origen (el front en localhost:5173)
// salvo que la API diga explícitamente "este origen tiene permiso".
const string FrontendCorsPolicy = "Frontend";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddPolicy(FrontendCorsPolicy, policy => policy
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()));

// ───────────────────────── 2. Pipeline HTTP (qué pasa con cada petición) ─────────────────────────
var app = builder.Build();

// El orden importa: el manejador de excepciones va primero para "envolver" todo lo demás.
app.UseExceptionHandler();
app.UseCors(FrontendCorsPolicy);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Crea/actualiza las tablas al arrancar. Siempre en desarrollo; en producción solo si se activa
// con la variable Database__MigrateOnStartup=true (útil para un demo con una sola instancia).
// En un sistema real las migraciones se aplican aparte, de forma controlada, antes de desplegar.
if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    await app.Services.MigrateShipmentsDatabaseAsync();
}

app.MapHealthChecks("/health");
app.MapShipmentEndpoints();

app.Run();
