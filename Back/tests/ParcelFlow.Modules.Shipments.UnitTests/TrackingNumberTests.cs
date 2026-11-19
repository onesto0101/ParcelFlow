using ParcelFlow.Modules.Shipments.Domain;

namespace ParcelFlow.Modules.Shipments.UnitTests;

public class TrackingNumberTests
{
    [Fact]
    public void Generate_ReturnsValidFormat()
    {
        var trackingNumber = TrackingNumber.Generate();

        Assert.Equal(TrackingNumber.TotalLength, trackingNumber.Value.Length);
        Assert.StartsWith(TrackingNumber.Prefix, trackingNumber.Value);
        Assert.All(trackingNumber.Value[TrackingNumber.Prefix.Length..], c => Assert.Contains(c, TrackingNumber.Alphabet));
    }

    [Fact]
    public void Generate_ManyTimes_ProducesDifferentValues()
    {
        var generated = Enumerable.Range(0, 1000).Select(_ => TrackingNumber.Generate().Value).ToHashSet();

        Assert.Equal(1000, generated.Count);
    }

    [Fact]
    public void From_LowercaseWithSpaces_IsNormalized()
    {
        var trackingNumber = TrackingNumber.From("  pf23456789ab ");

        Assert.Equal("PF23456789AB", trackingNumber.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("PF123")]                // muy corta
    [InlineData("XX23456789AB")]         // prefijo incorrecto
    [InlineData("PF23456789A0")]         // '0' no está en el alfabeto
    [InlineData("PF23456789ABC")]        // muy larga
    public void From_InvalidValue_ThrowsException(string value)
    {
        Assert.Throws<ArgumentException>(() => TrackingNumber.From(value));
    }

    [Fact]
    public void With_InvalidValue_ThrowsException()
    {
        var trackingNumber = TrackingNumber.Generate();

        Assert.Throws<ArgumentException>(() => trackingNumber with { Value = "invalid" });
    }

    [Fact]
    public void TwoTrackingNumbers_WithSameValue_AreEqual()
    {
        Assert.Equal(TrackingNumber.From("PF23456789AB"), TrackingNumber.From("pf23456789ab"));
    }
}
