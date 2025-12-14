namespace BO;

public class OpenOrderInList
{
    public int? CourierId { get; init; } // Identifier for the courier handling the order
    public int OrderId { get; init; } // Identifier for the order
    public OrderType TypeOrder { get; init; } // Type of the order
    public double Weight { get; init; }  // Weight of the order
    public string DeliveryAddress { get; init; } // Delivery address for the order
    public double ArealDistance { get; init; } // Aerial distance to the delivery address
    public double? ActualDistance { get; init; }    // Actual distance covered for the order
    public TimeSpan? ExpectedActualDeliveryTime { get; init; } // Expected time for actual delivery`
    public ScheduleStatus Status { get; init; } // Current schedule status of the order
    public TimeSpan TotalTimeLeft { get; init; } // Total time left for the order
    public DateTime MaxDeliveryTime { get; init; } // Maximum allowed delivery time for the order
}