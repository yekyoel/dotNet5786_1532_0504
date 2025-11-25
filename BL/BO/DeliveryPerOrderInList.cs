using DO;

namespace BO;

public class DeliveryPerOrderInList
{
    public int DeliveryId { get; init; }
    public int? CourierId { get; init; }
    public string CourierName { get; init; }
    OrderType TypeOrder { get; init; }
    DateTime OrderStart { get; init; }
    CompletionType? CompType { get; init; }
    DateTime? DeliveryEndTime { get; init; }
    }
