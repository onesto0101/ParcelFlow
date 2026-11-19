namespace ParcelFlow.Modules.Tracking.Domain;

public enum TrackingStatus
{
    Created = 1,
    PickedUp = 2,
    InTransit = 3,
    AtDistributionCenter = 4,
    OutForDelivery = 5,
    Delivered = 6,
    DeliveryFailed = 7,
    Cancelled = 8,
}
