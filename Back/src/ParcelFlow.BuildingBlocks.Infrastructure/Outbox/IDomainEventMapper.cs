using ParcelFlow.BuildingBlocks.Application.IntegrationEvents;
using ParcelFlow.BuildingBlocks.Domain;

namespace ParcelFlow.BuildingBlocks.Infrastructure.Outbox;

/// <summary>
/// Decide qué domain events de un módulo se publican hacia fuera, y con qué forma.
/// Cada módulo tiene el suyo (ShipmentsDomainEventMapper, TrackingDomainEventMapper...).
///
/// Devuelve null si el evento es solo interno: no todo lo que pasa dentro le interesa a los demás.
/// </summary>
public interface IDomainEventMapper
{
    IIntegrationEvent? Map(IDomainEvent domainEvent);
}
