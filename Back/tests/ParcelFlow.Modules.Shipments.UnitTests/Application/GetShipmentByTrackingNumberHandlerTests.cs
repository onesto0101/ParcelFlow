using ParcelFlow.BuildingBlocks.Application.Exceptions;
using ParcelFlow.Modules.Shipments.Application.GetShipmentByTrackingNumber;
using ParcelFlow.Modules.Shipments.Domain;

namespace ParcelFlow.Modules.Shipments.UnitTests.Application;

public class GetShipmentByTrackingNumberHandlerTests
{
    private readonly FakeShipmentRepository _repository = new();

    private GetShipmentByTrackingNumberHandler CreateHandler() => new(_repository);

    [Fact]
    public async Task Returns_the_shipment_as_dto()
    {
        var shipment = TestData.Shipment();
        _repository.Add(shipment);

        var dto = await CreateHandler().Handle(
            new GetShipmentByTrackingNumberQuery(shipment.TrackingNumber.Value), CancellationToken.None);

        Assert.Equal(shipment.Id, dto.Id);
        Assert.Equal("Registered", dto.Status);
        Assert.Equal("Guadalajara", dto.Recipient.Address.City);
        Assert.Equal(shipment.Cost.Amount, dto.Cost);
    }

    [Fact]
    public async Task Unknown_tracking_number_throws_NotFound()
    {
        var unknown = TrackingNumber.Generate().Value;

        await Assert.ThrowsAsync<NotFoundException>(
            () => CreateHandler().Handle(new GetShipmentByTrackingNumberQuery(unknown), CancellationToken.None));
    }

    [Fact]
    public async Task Malformed_tracking_number_throws_ArgumentException()
    {
        await Assert.ThrowsAnyAsync<ArgumentException>(
            () => CreateHandler().Handle(new GetShipmentByTrackingNumberQuery("hola"), CancellationToken.None));
    }
}
