using DO;

namespace BO;

public class DeliveryPerOrderInList
{
    public int DeliveryId { get; init; }
    public int? CourierId { get; init; }
    public string CourierName { get; init; }
    public OrderType TypeOrder { get; init; }
    public DateTime OrderStart { get; init; }
    public CompletionType? CompType { get; init; }
    public DateTime? DeliveryEndTime { get; init; }
    }
