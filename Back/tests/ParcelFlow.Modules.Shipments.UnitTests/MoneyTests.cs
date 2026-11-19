using ParcelFlow.Modules.Shipments.Domain;

namespace ParcelFlow.Modules.Shipments.UnitTests;

public class MoneyTests
{
    // --- Creación ---

    [Fact]
    public void Constructor_NegativeAmount_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Money(-0.01m, "MXN"));
    }

    [Fact]
    public void Constructor_ZeroAmount_IsValid()
    {
        var money = new Money(0m, "MXN");

        Assert.Equal(0m, money.Amount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("MX")]
    [InlineData("MXNN")]
    [InlineData("M1N")]
    public void Constructor_InvalidCurrency_ThrowsException(string currency)
    {
        Assert.Throws<ArgumentException>(() => new Money(10m, currency));
    }

    [Fact]
    public void Constructor_LowercaseCurrency_IsNormalizedToUppercase()
    {
        var money = new Money(10m, " mxn ");

        Assert.Equal("MXN", money.Currency);
    }

    [Fact]
    public void Constructor_RoundsToTwoDecimals_AwayFromZero()
    {
        Assert.Equal(10.01m, new Money(10.005m, "MXN").Amount);
        Assert.Equal(10.00m, new Money(10.004m, "MXN").Amount);
    }

    // --- Igualdad ---

    [Fact]
    public void TwoMoney_WithSameAmountAndCurrency_AreEqual()
    {
        Assert.Equal(new Money(10m, "MXN"), new Money(10m, "mxn"));
    }

    [Fact]
    public void TwoMoney_WithDifferentCurrency_AreNotEqual()
    {
        Assert.NotEqual(new Money(10m, "MXN"), new Money(10m, "USD"));
    }

    // --- Operaciones ---

    [Fact]
    public void Add_SameCurrency_ReturnsSum()
    {
        var result = new Money(10.50m, "MXN") + new Money(4.25m, "MXN");

        Assert.Equal(new Money(14.75m, "MXN"), result);
    }

    [Fact]
    public void Add_DifferentCurrency_ThrowsException()
    {
        var pesos = new Money(10m, "MXN");
        var dollars = new Money(10m, "USD");

        Assert.Throws<InvalidOperationException>(() => pesos + dollars);
    }

    [Fact]
    public void Multiply_RoundsResult()
    {
        // 15 × 1.2345 = 18.5175 → 18.52
        var result = new Money(15m, "MXN") * 1.2345m;

        Assert.Equal(18.52m, result.Amount);
    }

    [Fact]
    public void Multiply_NegativeFactor_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Money(15m, "MXN") * -1m);
    }

    [Fact]
    public void Operations_DoNotModifyOriginal()
    {
        var original = new Money(10m, "MXN");

        _ = original + new Money(5m, "MXN");

        Assert.Equal(10m, original.Amount);
    }
}
