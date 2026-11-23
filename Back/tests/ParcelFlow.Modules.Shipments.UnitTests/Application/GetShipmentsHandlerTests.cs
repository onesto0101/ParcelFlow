using ParcelFlow.BuildingBlocks.Application.Paging;
using ParcelFlow.Modules.Shipments.Application.GetShipments;
using ParcelFlow.Modules.Shipments.Domain;

namespace ParcelFlow.Modules.Shipments.UnitTests.Application;

public class GetShipmentsHandlerTests
{
    private readonly FakeShipmentQueries _queries = new();

    private GetShipmentsHandler Handler() => new(_queries);

    [Fact]
    public async Task Parses_status_ignoring_case_and_trims_search()
    {
        await Handler().Handle(new GetShipmentsQuery(Status: "cancelled", Search: "  monterrey "), CancellationToken.None);

        Assert.Equal(ShipmentStatus.Cancelled, _queries.LastList!.Value.Status);
        Assert.Equal("monterrey", _queries.LastList.Value.Search);
    }

    [Fact]
    public async Task Empty_filters_mean_no_filter_and_default_paging()
    {
        await Handler().Handle(new GetShipmentsQuery(Status: "", Search: "   "), CancellationToken.None);

        var (status, search, page) = _queries.LastList!.Value;
        Assert.Null(status);
        Assert.Null(search);
        Assert.Equal(new PageRequest(1, PageRequest.DefaultPageSize), page);
    }

    [Theory]
    [InlineData(0, 500, 1, PageRequest.MaxPageSize)]
    [InlineData(-3, 0, 1, 1)]
    [InlineData(4, 10, 4, 10)]
    public async Task Paging_is_clamped_to_safe_values(int page, int pageSize, int expectedPage, int expectedSize)
    {
        await Handler().Handle(new GetShipmentsQuery(Page: page, PageSize: pageSize), CancellationToken.None);

        Assert.Equal(new PageRequest(expectedPage, expectedSize), _queries.LastList!.Value.Page);
    }

    [Theory]
    [InlineData("Lost")]
    [InlineData("2")] // Enum.TryParse aceptaría números: EnumParsing no.
    public async Task Unknown_status_is_rejected(string status)
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            Handler().Handle(new GetShipmentsQuery(Status: status), CancellationToken.None));

        Assert.Equal("Status", ex.ParamName);
    }

    [Fact]
    public async Task Too_long_search_is_rejected()
    {
        var search = new string('a', GetShipmentsHandler.SearchMaxLength + 1);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            Handler().Handle(new GetShipmentsQuery(Search: search), CancellationToken.None));
    }
}
