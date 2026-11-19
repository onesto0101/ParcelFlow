namespace ParcelFlow.Modules.Shipments.Domain;

/// <summary>
/// Domain Service: calcula el costo de un envío.
///
/// Es un "servicio de dominio" porque la regla no pertenece a un solo value object:
/// necesita Weight Y Dimensions a la vez. Es estático y puro (mismas entradas → mismo
/// resultado, sin base de datos ni reloj), por eso es trivial de probar.
///
/// Regla: se cobra por el MAYOR entre el peso real y el peso volumétrico.
///   costo = tarifa base + kilos facturables × tarifa por kilo
/// </summary>
public static class ShippingCostCalculator
{
    public const string Currency = "MXN";
    public const decimal BaseFee = 50m;
    public const decimal RatePerKilogram = 15m;

    /// <summary>Kilos que se cobran: el mayor entre el peso real y el volumétrico.</summary>
    public static decimal BillableKilograms(Weight weight, Dimensions dimensions)
    {
        ArgumentNullException.ThrowIfNull(weight);
        ArgumentNullException.ThrowIfNull(dimensions);

        return Math.Max(weight.Kilograms, dimensions.VolumetricWeight);
    }

    public static Money Calculate(Weight weight, Dimensions dimensions)
    {
        var billable = BillableKilograms(weight, dimensions);

        var baseFee = new Money(BaseFee, Currency);
        var weightFee = new Money(RatePerKilogram, Currency) * billable;

        return baseFee + weightFee;
    }
}
