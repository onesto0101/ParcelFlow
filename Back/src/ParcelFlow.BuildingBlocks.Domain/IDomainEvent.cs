namespace ParcelFlow.BuildingBlocks.Domain;

/// <summary>
/// Algo que YA ocurrió en el dominio. Se nombra en pasado (ShipmentCreated, DeliveryAssigned).
/// Es interno al módulo; para comunicarse con otros módulos se traduce a un Integration Event.
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
