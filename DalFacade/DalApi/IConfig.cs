
namespace DalApi;
using DO;
/// <summary>
/// Interface for configuration settings in the DAL.
/// </summary>
public interface IConfig
{
    int NextDeliveryId { get; } // Gets the next delivery ID.
    DateTime Clock { get; set; }// Gets or sets the current clock time.
    double? MaxDist { get; set; }// Gets or sets the maximum distance for deliveries.

    double AvgCarMPH { get; set; }// Gets or sets the average car speed in miles per hour.
    double AvgMotorcycleMPH { get; set; }// Gets or sets the average motorcycle speed in miles per hour.
    double AvgBicycleMPH { get; set; }// Gets or sets the average bicycle speed in miles per hour.
    double AvgWalkMPH { get; set; }// Gets or sets the average walking speed in miles per hour.
    void Reset();// Resets the configuration settings to their default values.

}

