namespace BO;
/// <summary>
/// Methods of shipping available
/// </summary>
public enum ShippingMethod
{
    Car,
    Motorcycle,
    Bike,
    OnFoot,
    None
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
    Open, // Pending + not yet assigned to a courier
    InProgress, // Assigned to a courier
    Completed, // Delivered
    Rejected, // Closed delivery
    Cancelled // Cancelled before delivery
}

/// <summary>
/// Specifies the status of a scheduled item based on its timing relative to the planned schedule.
/// </summary>
/// <remarks>Use this enumeration to indicate whether a scheduled item is on time, at risk of being late, or
/// already late. The specific meaning of each value may depend on the context in which it is used.</remarks>
public enum ScheduleStatus
{
    OnTime, // The scheduled item is on track to meet its planned schedule.
    InRisk, // The scheduled item is at risk of being late.
    Late // The scheduled item is already
}

/// <summary>
/// Specifies the fields that can be used to filter or sort couriers in a list operation.
/// </summary>
/// <remarks>Use this enumeration to indicate which property of a courier should be used when applying filters or
/// ordering results in list queries. The available fields correspond to common courier attributes such as identifier,
/// name, activity status, employment date, and delivery statistics.</remarks>
public enum CourierInListFilter
{
   CourierId, // Identifier for the courier
   FullName, // Full name of the courier
   IsActive, // Indicates if the courier is currently active
   ShippingMethod, // Preferred shipping method of the courier
   EmploymentStartDate, // Employment start date of the courier
   TotalDelSuppliedOnTime, // Total number of deliveries supplied on time by the courier
   TotalLateDelSupplied,  // Total number of deliveries supplied late by the courier
   OrderId  // Current order ID being handled by the courier
}

/// <summary>
/// Specifies the fields by which orders in a list can be filtered or sorted.
/// </summary>
/// <remarks>Use this enumeration to indicate the property of an order to use when applying filtering or sorting
/// operations in order management scenarios. The available values correspond to common order attributes, such as
/// identifiers, status, scheduling, and time-related metrics.</remarks>
public enum OrderInListFilter
{
    DeliveryId, // Delivery identifier
    OrderId, // Order identifier
    OrderType, // Type of the order
    AerialDistance, // Aerial distance for the order
    OrderStatus, // Current status of the order
    ScheduleStatus, // Schedule status of the order
    TotalTimeLeft, //   Total time left for the order
    TotalCompletionTime, // Total time taken to complete the order
    TotalDeliveries // Total number of deliveries associated with the order
}  

/// <summary>
/// Specifies the available filters for listing closed deliveries.
/// </summary>
/// <remarks>Use this enumeration to select the property by which closed deliveries are filtered or sorted in list
/// operations. The specific meaning of each value corresponds to a property of a closed delivery, such as its
/// identifier, associated order, address, or completion details.</remarks>
public enum ClosedDeliveryInListFilter
{
    DeliveryId, // Identifier for the delivery
    OrderId, // Identifier for the associated order
    OrderType, // Type of the order
    DeliveryAddress, // Address where the delivery was made
    DeliveryType, // Type of delivery completion
    ActualDistance, // Actual distance covered during the delivery
    TotalCompletionTime, // Total time taken to complete the delivery
    CompletionType // Type of completion for the delivery
}

/// <summary>
/// Specifies the fields by which open orders can be filtered or sorted in a list operation.
/// </summary>
/// <remarks>Use this enumeration to indicate the property of an open order to filter or sort by when retrieving
/// order lists. The available fields correspond to common order attributes such as courier, order type, delivery
/// address, and timing information. The meaning of each value depends on the context in which the filter is
/// applied.</remarks>
public enum OpenOrderInListFilter
{
    CourierId, // Identifier for the courier handling the order
    OrderId, // Identifier for the order
    TypeOrder, // Type of the order
    Weight,     // Weight of the order
    DeliveryAddress,    // Address where the order is to be delivered
    ArealDistance, // Aerial distance to the delivery address
    ActualDistance, // Actual distance covered for the delivery
    ExpectedActualDeliveryTime, // Expected time for actual delivery
    Status, // Current status of the order
    TotalTimeLeft,  // Total time left for the order
    MaxDeliveryTime  // Maximum allowed delivery time for the order
}

/// <summary>
/// Specifies units of time for representing durations or intervals.
/// </summary>
/// <remarks>Use this enumeration to indicate the granularity of a time period, such as when scheduling events,
/// configuring timeouts, or aggregating data. The values range from minutes to years.</remarks>
public enum  Time
{
    Minute,
    Hour,
    Day,
    Month,
    Year
}
