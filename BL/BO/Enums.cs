namespace BO;
/// <summary>
/// Methods of shipping available
/// </summary>
public enum ShippingMethod
{
    Car,
    Motorcycle,
    Bike,
    OnFoot
}

/// <summary>
/// Represents the types of food available in the system.
/// </summary>
public enum OrderType
{
    Pizza,
    Hamburger,
    Fries,
    IceCream
}

/// <summary>
/// Types of delivery status
/// </summary>
public enum CompletionType
{
    Pending, // the client was unavailable at the time of delivery. The order goes back. The delivery closes but the order stays open
    Refused, // the courier reached the destination and the client refused the order (Order goes back)
    Delivered, // the order has been delivered and closed
    Cancelled, // the order was cancelled before delivery
    Failed // the delivery failed for a miscalculation of the distance
}

public enum OrderStatus
{
    Open, // Pending + 
    InProgress,
    Completed, // Delivered
    Rejected, // Closed delivery
    Cancelled
}

public enum ScheduleStatus
{
    OnTime,
    InRisk,
    Late
}

public enum CourierInListFilter
{
   CourierId,
   FullName,
   IsActive,
   TypeOrder,
   EmploymentStartDate,
   TotalDelSuppliedOnTime,
   TotalLateDelSupplied,
   OrderId 
}

public enum OrderInListFilter
{
    DeliveryId,
    OrderId,
    OrderType,
    AerialDistance,
    OrderStatus,
    ScheduleStatus,
    TotalTimeLeft,
    TotalCompletionTime,
    TotalDeliveries
}  

public enum ClosedDeliveryInListFilter
{
    DeliveryId,
    OrderId,
    OrderType,
    DeliveryAddress,
    DeliveryType,
    ActualDistance,
    TotalCompletionTime,
    CompletionType
}


public enum OpenOrderInListFilter
{
    CourierId,
    OrderId,
    TypeOrder,
    weight,
    DeliveryAddress,
    ArealDistance,
    ActualDistance,
    ExpectedActualDeliveryTime,
    status,
    TotalTimeLeft,
    MaxDeliveryTime 
}

public enum  Time
{
    Minute,
    Hour,
    Day,
    Month,
    Year
}
