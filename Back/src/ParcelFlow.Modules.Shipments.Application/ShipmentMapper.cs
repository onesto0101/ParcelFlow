using ParcelFlow.Modules.Shipments.Application.GetShipmentByTrackingNumber;
using ParcelFlow.Modules.Shipments.Domain;

namespace ParcelFlow.Modules.Shipments.Application;

internal static class ShipmentMapper
{
    public static ShipmentDto ToDto(this Shipment s) => new(
        s.Id,
        s.TrackingNumber.Value,
        s.Status.ToString(),
        s.Sender.ToDto(),
        s.Recipient.ToDto(),
        s.Weight.Kilograms,
        s.Dimensions.Length,
        s.Dimensions.Width,
        s.Dimensions.Height,
        s.Cost.Amount,
        s.Cost.Currency,
        s.CreatedAt,
        s.CancelledAt,
        s.CancellationReason,
        s.DeliveredAt);


    public static ContactDto ToDto(this Contact c) => new(
    c.Name,
    c.Phone,
    new AddressDto(c.Address.Street, c.Address.City, c.Address.State, c.Address.PostalCode, c.Address.Country));
    
}