namespace Dal;

internal static class Config
{
    internal const string s_data_config_xml = "data-config.xml";
    internal const string s_couriers_xml = "couriers.xml";
    internal const string s_orders_xml = "orders.xml";
    internal const string s_deliveries_xml = "deliveries.xml";

    internal static int NextDeliveryId
    {
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextDeliveryId");
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextDeliveryId", value);
    }

    internal static int NextOrderId
    {
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextOrderId");
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextOrderId", value);
    }

    internal static DateTime Clock
    {
        get => XMLTools.GetConfigDateVal(s_data_config_xml, "Clock");
        set => XMLTools.SetConfigDateVal(s_data_config_xml, "Clock", value);
    }

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
        NextDeliveryId = 1;
        NextOrderId = 1;
        AdminId = 0;
        Clock = DateTime.Now;
        CompanyName = null;
        Latitude = null;
        Longitude = null;
        MaxDist = null;
        AvgCarMPH = 0;
        AvgMotorcycleMPH = 0;
        AvgBicycleMPH = 0;
        AvgWalkMPH = 0;
        MaxDelTime = TimeSpan.Zero;
        RiskRange = TimeSpan.Zero;
        DownTime = TimeSpan.Zero;
    }

}
