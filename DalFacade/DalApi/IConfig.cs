
namespace DalApi;
using DO;
/// <summary>
/// Interface for configuration settings in the DAL.
/// </summary>
public interface IConfig
{
    //int NextDeliveryId { get; } // Gets the next delivery ID. check!!
    // int NextOrderId { get; } // Gets the next order ID. check!!
    DateTime Clock { get; set; }// Gets or sets the current clock time.

    int AdminId { get; set; }// Gets the administrator ID.

    string CompanyName { get; set; }// Gets or sets the company name.

    double? Longitude { get; set; }// Gets or sets the longitude.

    double? Latitude { get; set; }// Gets or sets the latitude.

    double? MaxDist { get; set; }// Gets or sets the maximum distance for deliveries.

    double AvgCarMPH { get; set; }// Gets or sets the average car speed in miles per hour.
    double AvgMotorcycleMPH { get; set; }// Gets or sets the average motorcycle speed in miles per hour.
    double AvgBicycleMPH { get; set; }// Gets or sets the average bicycle speed in miles per hour.
    double AvgWalkMPH { get; set; }// Gets or sets the average walking speed in miles per hour.

    TimeSpan MaxDelTime { get; set; }// Gets or sets the maximum delivery time.

    TimeSpan RiskRange { get; set; }// Gets or sets the risk range time.

    TimeSpan DownTime { get; set; }// Gets or sets the downtime duration.

    void Reset();// Resets the configuration settings to their default values.

}

