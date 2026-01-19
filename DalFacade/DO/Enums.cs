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
    Pending, //( ממתין ,(זה כולל מקרה שבוא השליח הגיע ליעד אבל הלקוח לא היה בבית in this case the order closes and opens again 
    Refused, // מזמין סירב לקבל
    Delivered, //סופק
    Cancelled, //בוטל
    Failed // the delivery failed casue of the dist calc the delivery closes and the order stays open
}