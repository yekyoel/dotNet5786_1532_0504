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
    Pending,
    EnRoute,
    Delivered,
    Cancelled,
    Failed
}

public enum OrderStatus
{
    Open,
    InProgress,
    Completed,
    Rejected,
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