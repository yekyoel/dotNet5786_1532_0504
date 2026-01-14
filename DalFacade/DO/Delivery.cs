// Module Delivery.cs
namespace DO;

/// <summary>
/// Delivery entity representing a delivery record in the system.
/// </summary>
/// <param name="Id">unique ID of the delivery</param>
/// <param name="OrderId">ID of the order being delivered</param>
/// <param name="CourierId">ID of the courier assigned to the delivery</param>
/// <param name="ShippingMethod">Method of shipping used for the delivery</param>
/// <param name="DeliveryStartTime">Time when the delivery started</param>
/// <param name="Distance">the actual Distance during the delivery</param>
/// <param name="End">Status of the delivery</param>
/// <param name="DeliveryEndTime">Time when the delivery ended</param>
public record Delivery
(
    int Id, 
    int OrderId,
    int CourierId,
    ShippingMethod? ShippingMethod = null, 
    DateTime? DeliveryStartTime = null,  
    double? Distance = null, 
    CompletionType? End = null, 
    DateTime? DeliveryEndTime = null  
)
{
    /// <summary>
    /// Default constructor for Courier with default values.
    /// initializes non empty "?" properties to their default values.
    /// <summary>
    public Delivery() :this(0,0,0) { }
}