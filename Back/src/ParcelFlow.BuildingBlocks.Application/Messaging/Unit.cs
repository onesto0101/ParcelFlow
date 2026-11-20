namespace ParcelFlow.BuildingBlocks.Application.Messaging;

/// <summary>
/// "Nada" como tipo. En C# no se puede escribir ICommand&lt;void&gt;, así que los comandos
/// que no devuelven nada (ej. CancelShipment) usan ICommand&lt;Unit&gt; y su handler
/// termina con "return Unit.Value;".
/// </summary>
public readonly record struct Unit
{
    public static readonly Unit Value = new();
}
