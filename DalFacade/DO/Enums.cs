namespace DO;

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
public enum  OrderType
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