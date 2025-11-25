namespace BO;

public class Order
{
    public int Id { get; init; }
    OrderType orderType { get; set; }
    public string Description { get; set; }
    public string OrderAddress { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double AerialDistance { get; set; }
    public string CustomerName { get; set; }
    public string CustomerPhone { get; set; }
    public double Weight { get; set; }
    public DateTime OrderPlacedTime { get; init; }
    public DateTime? ExpectedDeliveryTime { get; init; }
    public DateTime MaxDeliveredTime { get; init; }
    public OrderStatus OrderStatus { get; init; }
    public ScheduleStatus ScheduleStatus { get; init; }
    public TimeSpan TotalTimeLeft { get; init; }
    public List<DeliveryPerOrderInList>? DeliveriesList { get; init; }

}
