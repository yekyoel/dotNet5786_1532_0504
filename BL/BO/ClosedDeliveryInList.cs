namespace BO;
public class ClosedDeliveryInList
{
    public int DeliveryId { get; init; }
    public int OrderId { get; init; }
    public OrderType OrderType { get; init; }
    public string DeliveryAddress { get; init; }
    public ShippingMethod DeliveryType { get; init; }
    public double ActualDistance { get; init; }
    public TimeSpan TotalCompletionTime { get; init; }
    public CompletionType CompletionType { get; init; }

}
