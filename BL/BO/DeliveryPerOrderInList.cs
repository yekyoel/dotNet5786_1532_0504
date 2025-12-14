using DO;

namespace BO;

public class DeliveryPerOrderInList
{
    public int DeliveryId { get; init; } // Identifier for the delivery
    public int? CourierId { get; init; } // Identifier for the courier handling the delivery
    public string CourierName { get; init; } // Name of the courier handling the delivery
    public OrderType TypeOrder { get; init; } // Type of the order associated with the delivery
    public DateTime OrderStart { get; init; } // Start time of the order
    public CompletionType? CompType { get; init; } // Completion type of the delivery
    public DateTime? DeliveryEndTime { get; init; } // End time of the delivery
}
