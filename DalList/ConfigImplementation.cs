using Dal;
namespace DalApi;

/// <summary>
/// Config Implementation class that implements the IConfig interface for managing configuration settings.
/// </summary>
internal class ConfigImplementation : IConfig
{
    // Properties to get and set configuration values from the Config class.
    public DateTime Clock
    {
        get => Config.Clock;
        set => Config.Clock = value;
    }
    public int AdminId
    {
        get => Config.AdminId;
        set => Config.AdminId = value;
    }
    public string CompanyName
    {
        get => Config.CompanyName ?? string.Empty;
        set => Config.CompanyName = value;
    }
    public double? Longitude
    {
        get => Config.Longitude;
        set => Config.Longitude = value;
    }
    public double? Latitude
    {
        get => Config.Latitude;
        set => Config.Latitude = value;
    }
    public double? MaxDist
    {
        get => Config.MaxDist;
        set => Config.MaxDist = value;
    }
   // public int NextDeliveryId
   // {
    //    get => Config.NextDeliveryId;
    //}
    public double AvgCarMPH
    {
        get => Config.AvgCarMPH;
        set => Config.AvgCarMPH = value;
    }
    public double AvgMotorcycleMPH
    {
        get => Config.AvgMotorcycleMPH;
        set => Config.AvgMotorcycleMPH = value;
    }
    public double AvgBicycleMPH
    {
        get => Config.AvgBicycleMPH;
        set => Config.AvgBicycleMPH = value;
    }
    public double AvgWalkMPH
    {
        get => Config.AvgWalkMPH;
        set => Config.AvgWalkMPH = value;
    }


    public TimeSpan MaxDelTime
    {
        get => Config.MaxDelTime;
        set => Config.MaxDelTime = value;
    }

    public TimeSpan RiskRange
    {
        get => Config.RiskRange;
        set => Config.RiskRange = value;
    }

    public TimeSpan DownTime
    {
        get => Config.DownTime;
        set => Config.DownTime = value;
    }

    // resets the configuration settings to their default values.
    public void Reset()
    {
        Config.Reset();
    }

}

/*    public int AdminId
    {
        get => Config.AdminId;
        set => Config.AdminId = value;
    }

    public string CompanyName
    {
        get => Config.CompanyName!;
        set => Config.CompanyName = value;
    }

    public double? Latitude
    {
        get => Config.Latitude;
        set => Config.Latitude = value;
    }

    public double? Longitude
    {
        get => Config.Longitude;
        set => Config.Longitude = value;
    }
*/