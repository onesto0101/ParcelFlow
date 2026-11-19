using ParcelFlow.BuildingBlocks.Domain;

namespace ParcelFlow.Modules.Deliveries.Domain;

/// <summary>
/// Prueba de una entrega exitosa: quién recibió el paquete (DE-008) y notas opcionales.
/// Mismo estilo que FailureReason: propiedades { get; } validadas en el constructor.
/// </summary>
public sealed record DeliveryEvidence
{
    public const int ReceivedByMinLength = 2;
    public const int ReceivedByMaxLength = 150;
    public const int NotesMaxLength = 500;

    public string ReceivedBy { get; }
    public string? Notes { get; }

    public DeliveryEvidence(string receivedBy, string? notes = null)
    {
        ReceivedBy = Guard.RequiredText(receivedBy, nameof(receivedBy), ReceivedByMaxLength, ReceivedByMinLength);
        Notes = Guard.OptionalText(notes, nameof(notes), NotesMaxLength);
    }
}
