namespace BO;

public class OpenOrderInList
{
    public int? CourierId { get; init; }
    public int OrderId { get; init; }
    OrderType TypeOrder { get; init; }
    public double weight { get; init; }
    public string DeliveryAddress { get; init; }
    public double ArealDistance { get; init; }
    public double? ActualDistance { get; init; }
    public TimeSpan? ExpectedActualDeliveryTime { get; init; }
    public ScheduleStatus status { get; init; }
    public TimeSpan TotalTimeLeft { get; init; }
    public DateTime MaxDeliveryTime { get; init; }
}