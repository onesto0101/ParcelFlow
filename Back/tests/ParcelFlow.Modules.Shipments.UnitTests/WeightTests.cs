using ParcelFlow.Modules.Shipments.Domain;

namespace ParcelFlow.Modules.Shipments.UnitTests;

public class WeightTests
{
    // --- Valores inválidos ---

    [Fact]
    public void Constructor_NegativeValue_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Weight(-1m)
        );
    }

    [Fact]
    public void Constructor_ZeroValue_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Weight(Weight.MinKilograms)
        );
    }

    [Fact]
    public void Constructor_JustAboveMax_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Weight(Weight.MaxKilograms + 0.01m)
        );
    }

    [Fact]
    public void Constructor_InvalidValue_ExceptionContainsRejectedValue()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new Weight(Weight.MaxKilograms + 1.3m)
        );

        Assert.Equal(Weight.MaxKilograms + 1.3m, exception.ActualValue);
    }

    // --- Valores válidos (bordes) ---

    [Fact]
    public void Constructor_JustAboveMin_IsValid()
    {
        var weight = new Weight(0.01m);

        Assert.Equal(0.01m, weight.Kilograms);
    }

    [Fact]
    public void Constructor_ExactlyMax_IsValid()
    {
        var weight = new Weight(Weight.MaxKilograms);

        Assert.Equal(Weight.MaxKilograms, weight.Kilograms);
    }

    // --- Igualdad por valor ---

    [Fact]
    public void TwoWeights_WithSameValue_AreEqual()
    {
        var weight1 = new Weight(5m);
        var weight2 = new Weight(5m);

        Assert.Equal(weight1, weight2);
    }

    [Fact]
    public void TwoWeights_WithDifferentValue_AreNotEqual()
    {
        var weight1 = new Weight(5m);
        var weight2 = new Weight(10m);

        Assert.NotEqual(weight1, weight2);
    }

    // --- Inmutabilidad / with ---

    [Fact]
    public void With_InvalidValue_ThrowsException()
    {
        var weight = new Weight(5m);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => weight with { Kilograms = 0m }
        );
    }
}
