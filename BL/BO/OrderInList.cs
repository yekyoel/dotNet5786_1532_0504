namespace BO;
public class OrderInList
{
    public int? DeliveryId { get; init; }
    public int OrderId { get; init; }
    public OrderType OrderType { get; init; }
    public double AerialDistance { get; init; }
    public OrderStatus OrderStatus { get; init; }
    public ScheduleStatus ScheduleStatus { get; init; }
    public TimeSpan TotalTimeLeft { get; init; }
    public TimeSpan TotalCompletionTime { get; init; }
    public int TotalDeliveries { get; init; }

}
