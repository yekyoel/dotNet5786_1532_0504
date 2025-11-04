
namespace Dal;

internal static class Config
{
    internal const int deliveryId = 0;  ///the number of deliveries 
    private static int delId = deliveryId;
    internal static int NextDeliveryId { get => delId++; }

    internal const int orderId = 0;     ///the num of orders
    private static int ordId = orderId;
    internal static int NextOrderId { get => ordId++; }

    internal static DateTime Clock { get; set; } = DateTime.Now;
   
    internal static int AdminId;

    internal static string? CompanyName = null;

    internal static double? Latitude = null;
    internal static double? Longitude = null;
    internal static double? MaxDistance = null;
    internal static double AvgCarMPH;
    internal static double AvgMotorBikeMPH;
    internal static double AvgBikeMPH;
    internal static double AvgWalkMPH;

    internal static TimeSpan MaxDelTime;
    internal static TimeSpan RiskRange;
    internal static TimeSpan DownTime;

    internal static void Reset()
    {
        delId = deliveryId;
        ordId = orderId;
        AdminId = 0;
        Clock = DateTime.Now;
        CompanyName = null;
        Latitude = null; 
        Longitude = null;
        MaxDistance = null;
        AvgCarMPH = 0;
        AvgMotorBikeMPH = 0 ;
        AvgBikeMPH = 0;
        AvgWalkMPH = 0;
        MaxDelTime = TimeSpan.Zero;
        RiskRange = TimeSpan.Zero;
        DownTime = TimeSpan.Zero;
    }
}

