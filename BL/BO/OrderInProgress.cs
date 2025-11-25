using DO;

namespace BO;

public class OrderInProgress
{
    public int DeliveryId { get; init; }

    public int OrderId { get; init; }

    public OrderType OrderType { get; init; }

    public string? Description { get; init; }

    public string DeliveryAddress { get; init; }

    public double ArealDistance { get; init; }

    public double? ActualDistance { get; init; }

    public string CustomerName { get; init; }

    public string CustomerNumber { get; init; }

    public DateTime OrderPlacedTime { get; init; }

    public DateTime DeliveryTime { get; init; }

    public DateTime ExpectedDeliveryTime { get; init; }

    public DateTime MaxDeliveryTime { get; init; }

    //OrderStatus OrderStats { get; init; }    

    // ScheduleStatus ScheduleStat { get; init; }

   public  TimeSpan TotalTimeLeft { get; init; }
}
