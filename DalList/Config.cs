
namespace Dal;
/// <summary>
/// Class for configuration settings in the DAL.
/// </summary> 

internal static class Config
{
    internal const int deliveryId = 1;  ///the number of deliveries 
    private static int delId = deliveryId; //counter for delivery IDs
    internal static int NextDeliveryId { get => delId++; } //property to get the next delivery ID

    internal const int orderId = 1;     ///the num of orders
    private static int ordId = orderId; //counter for order IDs
    internal static int NextOrderId { get => ordId++; } //property to get the next order ID

    internal static DateTime Clock { get; set; } = DateTime.Now; //the current time in the system

    internal static int AdminId; // the admin ID

    internal static string? CompanyName = null; //the company name

    internal static double? Latitude = null; //the latitude
    internal static double? Longitude = null; //the longitude
    internal static double? MaxDist = null; //the maximum distance for deliveries
    internal static double AvgCarMPH; //the average car speed in miles per hour
    internal static double AvgMotorcycleMPH; //the average motorcycle speed in miles per hour
    internal static double AvgBicycleMPH; //the average bicycle speed in miles per hour
    internal static double AvgWalkMPH; //the average walking speed in miles per hour

    internal static TimeSpan MaxDelTime; //the maximum delivery time
    internal static TimeSpan RiskRange; //the risk range time
    internal static TimeSpan DownTime; //the downtime duration


    /// <summary>
    /// this method resets the configuration settings to their default values.
    /// </summary>
    internal static void Reset()
    {
        delId = deliveryId;
        ordId = orderId;
        AdminId = 0;
        Clock = DateTime.Now;
        CompanyName = null;
        Latitude = null; 
        Longitude = null;
        MaxDist = null;
        AvgCarMPH = 0;
        AvgMotorcycleMPH = 0 ;
        AvgBicycleMPH = 0;
        AvgWalkMPH = 0;
        MaxDelTime = TimeSpan.Zero;
        RiskRange = TimeSpan.Zero;
        DownTime = TimeSpan.Zero;
    }
}

