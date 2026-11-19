using ParcelFlow.Modules.Shipments.Domain;

namespace ParcelFlow.Modules.Shipments.UnitTests;

public class DimensionsTests
{
    private const decimal Valid = 50m;
    private const decimal BelowMin = Dimensions.MinCentimeters - 0.01m;
    private const decimal AboveMax = Dimensions.MaxCentimeters + 0.01m;

    // --- Valores inválidos ---

    public static TheoryData<decimal, decimal, decimal, string> InvalidDimensions => new()
    {
        { BelowMin, Valid, Valid, nameof(Dimensions.Length) },
        { Valid, BelowMin, Valid, nameof(Dimensions.Width) },
        { Valid, Valid, BelowMin, nameof(Dimensions.Height) },
        { AboveMax, Valid, Valid, nameof(Dimensions.Length) },
        { Valid, AboveMax, Valid, nameof(Dimensions.Width) },
        { Valid, Valid, AboveMax, nameof(Dimensions.Height) },
    };

    [Theory]
    [MemberData(nameof(InvalidDimensions))]
    public void Constructor_ValueOutOfRange_ThrowsWithPropertyName(
        decimal length, decimal width, decimal height, string expectedParamName)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new Dimensions(length, width, height)
        );

        Assert.Equal(expectedParamName, exception.ParamName);
    }

    [Fact]
    public void Constructor_InvalidValue_ExceptionContainsRejectedValue()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new Dimensions(AboveMax, Valid, Valid)
        );

        Assert.Equal(AboveMax, exception.ActualValue);
    }

    // --- Valores válidos (bordes) ---

    [Fact]
    public void Constructor_ExactlyMin_IsValid()
    {
        var min = Dimensions.MinCentimeters;

        var dimensions = new Dimensions(min, min, min);

        Assert.Equal(min, dimensions.Length);
        Assert.Equal(min, dimensions.Width);
        Assert.Equal(min, dimensions.Height);
    }

    [Fact]
    public void Constructor_ExactlyMax_IsValid()
    {
        var max = Dimensions.MaxCentimeters;

        var dimensions = new Dimensions(max, max, max);

        Assert.Equal(max, dimensions.Length);
        Assert.Equal(max, dimensions.Width);
        Assert.Equal(max, dimensions.Height);
    }

    // --- Igualdad por valor ---

    [Fact]
    public void TwoDimensions_WithSameValues_AreEqual()
    {
        var dimensions1 = new Dimensions(10m, 20m, 30m);
        var dimensions2 = new Dimensions(10m, 20m, 30m);

        Assert.Equal(dimensions1, dimensions2);
    }

    [Fact]
    public void TwoDimensions_WithDifferentOrientation_AreNotEqual()
    {
        var dimensions1 = new Dimensions(10m, 20m, 30m);
        var dimensions2 = new Dimensions(30m, 20m, 10m);

        Assert.NotEqual(dimensions1, dimensions2);
    }

    // --- Inmutabilidad / with ---

    [Fact]
    public void With_InvalidValue_ThrowsException()
    {
        var dimensions = new Dimensions(Valid, Valid, Valid);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => dimensions with { Height = AboveMax }
        );
    }

    // --- Cálculos ---

    [Fact]
    public void Volume_ReturnsLengthTimesWidthTimesHeight()
    {
        var dimensions = new Dimensions(10m, 20m, 30m);

        Assert.Equal(6000m, dimensions.Volume);
    }

    [Fact]
    public void VolumetricWeight_ReturnsVolumeDividedByDivisor()
    {
        // 50 × 40 × 30 = 60 000 cm³ → 60 000 / 5000 = 12 kg
        var dimensions = new Dimensions(50m, 40m, 30m);

        Assert.Equal(12m, dimensions.VolumetricWeight);
    }

    [Fact]
    public void VolumetricWeight_CanExceedMaxPhysicalWeight()
    {
        // 150 × 150 × 150 / 5000 = 675 kg: por eso devuelve decimal y no Weight.
        var max = Dimensions.MaxCentimeters;
        var dimensions = new Dimensions(max, max, max);

        Assert.True(dimensions.VolumetricWeight > Weight.MaxKilograms);
    }
}
