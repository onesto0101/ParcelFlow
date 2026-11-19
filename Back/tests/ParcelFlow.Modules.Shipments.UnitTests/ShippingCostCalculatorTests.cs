using ParcelFlow.Modules.Shipments.Domain;

namespace ParcelFlow.Modules.Shipments.UnitTests;

public class ShippingCostCalculatorTests
{
    [Fact]
    public void BillableKilograms_HeavySmallBox_UsesRealWeight()
    {
        // Volumétrico: 30 × 20 × 10 / 5000 = 1.2 kg < 5 kg reales
        var billable = ShippingCostCalculator.BillableKilograms(new Weight(5m), new Dimensions(30m, 20m, 10m));

        Assert.Equal(5m, billable);
    }

    [Fact]
    public void BillableKilograms_LightBigBox_UsesVolumetricWeight()
    {
        // Volumétrico: 50 × 40 × 30 / 5000 = 12 kg > 2 kg reales
        var billable = ShippingCostCalculator.BillableKilograms(new Weight(2m), new Dimensions(50m, 40m, 30m));

        Assert.Equal(12m, billable);
    }

    [Fact]
    public void Calculate_ReturnsBaseFeePlusRateTimesBillableKilograms()
    {
        // 50 + 12 × 15 = 230 MXN
        var cost = ShippingCostCalculator.Calculate(new Weight(2m), new Dimensions(50m, 40m, 30m));

        var expected = ShippingCostCalculator.BaseFee + 12m * ShippingCostCalculator.RatePerKilogram;
        Assert.Equal(new Money(expected, ShippingCostCalculator.Currency), cost);
    }
}
