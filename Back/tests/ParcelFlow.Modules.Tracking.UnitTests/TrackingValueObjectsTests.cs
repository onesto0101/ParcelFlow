using ParcelFlow.Modules.Tracking.Domain;

namespace ParcelFlow.Modules.Tracking.UnitTests;

public class TrackingValueObjectsTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("A")]
    public void Location_InvalidName_ThrowsException(string name)
    {
        Assert.Throws<ArgumentException>(() => new Location(name));
    }

    [Fact]
    public void Location_TooLong_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => new Location(new string('a', Location.MaxLength + 1)));
    }

    [Fact]
    public void Location_SameName_AreEqual()
    {
        Assert.Equal(new Location("CEDIS Monterrey"), new Location("  CEDIS Monterrey "));
    }

    [Fact]
    public void TrackingDescription_Empty_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => new TrackingDescription("   "));
    }

    [Fact]
    public void TrackingDescription_With_InvalidValue_ThrowsException()
    {
        var description = new TrackingDescription("Recibido en almacén");

        Assert.Throws<ArgumentException>(() => description with { Value = "" });
    }
}
