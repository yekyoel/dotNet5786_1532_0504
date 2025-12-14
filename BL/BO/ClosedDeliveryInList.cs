namespace BO;
public class ClosedDeliveryInList
{
    public int DeliveryId { get; init; } // Identifier for the closed delivery
    public int OrderId { get; init; } // Identifier of the associated order
    public OrderType OrderType { get; init; } // Type of the order (e.g., Standard, Express)
    public string DeliveryAddress { get; init; } // Address where the delivery was made
    public ShippingMethod DeliveryType { get; init; } // Method used for delivery (e.g., Air, Ground)
    public double ActualDistance { get; init; } // Actual distance covered during the delivery in kilometers
    public TimeSpan TotalCompletionTime { get; init; } // Total time taken to complete the delivery
    public CompletionType CompletionType { get; init; } // Type of completion (e.g., OnTime, Late)

}
