using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using ParcelFlow.BuildingBlocks.Application.Messaging;
using ParcelFlow.Modules.Shipments.Application;
using ParcelFlow.Modules.Shipments.Application.Abstractions;
using ParcelFlow.Modules.Shipments.Application.GetShipmentByTrackingNumber;

namespace ParcelFlow.Modules.Shipments.UnitTests.Application;

/// <summary>
/// Prueba "de cableado": arma el contenedor de DI como lo hará la API y comprueba que
/// el mediator encuentra los handlers registrados por AddShipmentsApplication().
/// </summary>
public class ShipmentsApplicationModuleTests
{
    private static ServiceProvider BuildProvider(FakeShipmentRepository repository) =>
        new ServiceCollection()
            .AddShipmentsApplication()
            .AddSingleton<IShipmentRepository>(repository)
            .AddSingleton<IShipmentsUnitOfWork, FakeShipmentsUnitOfWork>()
            .AddSingleton<IShipmentQueries, FakeShipmentQueries>()
            // Los handlers de integration events piden ILogger<T>; en la API lo registra ASP.NET.
            .AddSingleton(typeof(ILogger<>), typeof(NullLogger<>))
            .AddSingleton<TimeProvider>(new FakeTimeProvider(new DateTimeOffset(TestData.Now)))
            .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });

    [Fact]
    public async Task Mediator_routes_command_and_query_to_their_handlers()
    {
        var repository = new FakeShipmentRepository();
        using var provider = BuildProvider(repository);
        using var scope = provider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var created = await mediator.Send(CommandData.CreateShipment());
        var dto = await mediator.Query(new GetShipmentByTrackingNumberQuery(created.TrackingNumber));

        Assert.Equal(created.ShipmentId, dto.Id);
    }
}
