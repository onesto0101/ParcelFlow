using ParcelFlow.Modules.Deliveries.Domain;

namespace ParcelFlow.Modules.Deliveries.UnitTests;

public class FailureReasonTests
{
    [Fact]
    public void Constructor_KnownCodeWithoutDetails_IsValid()
    {
        var reason = new FailureReason(DeliveryFailureCode.RecipientAbsent);

        Assert.Equal(DeliveryFailureCode.RecipientAbsent, reason.Code);
        Assert.Null(reason.Details);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_OtherWithoutDetails_ThrowsException(string? details)
    {
        Assert.Throws<ArgumentException>(() => new FailureReason(DeliveryFailureCode.Other, details));
    }

    [Fact]
    public void Constructor_OtherWithDetails_IsValid()
    {
        var reason = new FailureReason(DeliveryFailureCode.Other, "  Perro bravo en la entrada ");

        Assert.Equal("Perro bravo en la entrada", reason.Details);
    }

    [Fact]
    public void Constructor_UndefinedCode_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new FailureReason((DeliveryFailureCode)999));
    }

    [Fact]
    public void TwoReasons_WithSameData_AreEqual()
    {
        Assert.Equal(
            new FailureReason(DeliveryFailureCode.WrongAddress, "No existe el número"),
            new FailureReason(DeliveryFailureCode.WrongAddress, "No existe el número"));
    }
}

public class DeliveryEvidenceAndCourierTests
{
    [Fact]
    public void DeliveryEvidence_WithoutReceiver_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => new DeliveryEvidence(" "));
    }

    [Fact]
    public void DeliveryEvidence_EmptyNotes_AreStoredAsNull()
    {
        var evidence = new DeliveryEvidence("María Gómez", "   ");

        Assert.Null(evidence.Notes);
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    public void CourierId_Invalid_ThrowsException(string value)
    {
        Assert.Throws<ArgumentException>(() => new CourierId(value));
    }

    [Fact]
    public void CourierId_SameValue_AreEqual()
    {
        Assert.Equal(new CourierId("REP-014"), new CourierId(" REP-014 "));
    }
}
