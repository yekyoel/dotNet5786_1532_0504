namespace BO;

public class Config
{
    public DateTime Clock { get; set; }// Gets or sets the current clock time.
    public int AdminId { get; set; }// Gets the administrator ID.
    public string CompanyName { get; set; }// Gets or sets the company name.
    public double? Longitude { get; set; }// Gets or sets the longitude.
    public double? Latitude { get; set; }// Gets or sets the latitude.
    public double? MaxDist { get; set; }// Gets or sets the maximum distance for deliveries.
    public double AvgCarMPH { get; set; }// Gets or sets the average car speed in miles per hour.
    public double AvgMotorcycleMPH { get; set; }// Gets or sets the average motorcycle speed in miles per hour.
    public double AvgBicycleMPH { get; set; }// Gets or sets the average bicycle speed in miles per hour.
    public double AvgWalkMPH { get; set; }// Gets or sets the average walking speed in miles per hour.
    public TimeSpan MaxDelTime { get; set; }// Gets or sets the maximum delivery time.
    public TimeSpan RiskRange { get; set; }// Gets or sets the risk range time.
    public TimeSpan DownTime { get; set; }// Gets or sets the downtime duration.
}
