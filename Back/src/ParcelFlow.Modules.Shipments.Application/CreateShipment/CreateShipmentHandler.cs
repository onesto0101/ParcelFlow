using ParcelFlow.BuildingBlocks.Application.Messaging;
using ParcelFlow.Modules.Shipments.Application.Abstractions;
using ParcelFlow.Modules.Shipments.Domain;

namespace ParcelFlow.Modules.Shipments.Application.CreateShipment;

/// <summary>
/// Orquesta el caso de uso. Fíjate que NO tiene reglas de negocio: solo coordina.
///   1. Convierte los datos crudos en value objects (si algo es inválido, lanzan ArgumentException → 400).
///   2. Le pide al dominio que cree el envío (el dominio calcula el costo y lanza el evento).
///   3. Lo añade al repositorio y guarda.
/// </summary>
internal sealed class CreateShipmentHandler(
    IShipmentRepository repository,
    IShipmentsUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<CreateShipmentCommand, CreateShipmentResult>
{
    public async Task<CreateShipmentResult> Handle(CreateShipmentCommand command, CancellationToken cancellationToken)
    {
        var shipment = Shipment.Create(
            TrackingNumber.Generate(),
            ToContact(command.Sender),
            ToContact(command.Recipient),
            new Weight(command.WeightKg),
            new Dimensions(command.LengthCm, command.WidthCm, command.HeightCm),
            timeProvider.GetUtcNow().UtcDateTime);

        repository.Add(shipment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateShipmentResult(
            shipment.Id,
            shipment.TrackingNumber.Value,
            shipment.Cost.Amount,
            shipment.Cost.Currency);
    }

    private static Contact ToContact(ContactInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(input.Address);

        var address = new Address(
            input.Address.Street,
            input.Address.City,
            input.Address.State,
            input.Address.PostalCode,
            input.Address.Country);

        return new Contact(input.Name, input.Phone, address);
    }
}
