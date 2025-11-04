

using Dal;
namespace DalApi;

/// <summary>
/// Config Implementation class that implements the IConfig interface for managing configuration settings.
/// </summary>
public class ConfigImplementation : IConfig
{
    // Properties to get and set configuration values from the Config class.
    public DateTime Clock
    {
        get => Config.Clock;
        set => Config.Clock = value;
    }
    public double? MaxDist
    {
        get => Config.MaxDist;
        set => Config.MaxDist = value;
    }
    public int NextDeliveryId
    {
        get => Config.NextDeliveryId;
    }
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
    // resets the configuration settings to their default values.
    public void Reset()
    {
        Config.Reset();
    }

}
