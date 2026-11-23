namespace ParcelFlow.BuildingBlocks.Application.IntegrationEvents;

/// <summary>
/// Un hecho que un módulo PUBLICA para que otros módulos reaccionen
/// (ej. "se creó un envío" → Tracking empieza el seguimiento).
///
/// Diferencia con IDomainEvent:
/// - Domain event      → interno del módulo, usa tipos del dominio, puede cambiar cuando quieras.
/// - Integration event → contrato PÚBLICO entre módulos: solo tipos primitivos (Guid, string...),
///                       vive en un proyecto aparte (*.IntegrationEvents) y cambiarlo rompe a otros.
/// </summary>
public interface IIntegrationEvent
{
    DateTime OccurredOnUtc { get; }
}
