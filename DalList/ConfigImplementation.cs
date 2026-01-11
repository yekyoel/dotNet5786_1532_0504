using System.Runtime.CompilerServices;
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
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => Config.Clock;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => Config.Clock = value;
    }

    public int AdminId
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => Config.AdminId;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => Config.AdminId = value;
    }

    public string CompanyName
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => Config.CompanyName ?? string.Empty;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => Config.CompanyName = value;
    }

    public double? Longitude
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => Config.Longitude;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => Config.Longitude = value;
    }

    public double? Latitude
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => Config.Latitude;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => Config.Latitude = value;
    }

    public double? MaxDist
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => Config.MaxDist;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => Config.MaxDist = value;
    }

    public double AvgCarMPH
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => Config.AvgCarMPH;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => Config.AvgCarMPH = value;
    }

    public double AvgMotorcycleMPH
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => Config.AvgMotorcycleMPH;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => Config.AvgMotorcycleMPH = value;
    }

    public double AvgBicycleMPH
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => Config.AvgBicycleMPH;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => Config.AvgBicycleMPH = value;
    }

    public double AvgWalkMPH
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => Config.AvgWalkMPH;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => Config.AvgWalkMPH = value;
    }

    public TimeSpan MaxDelTime
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => Config.MaxDelTime;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => Config.MaxDelTime = value;
    }

    public TimeSpan RiskRange
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => Config.RiskRange;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => Config.RiskRange = value;
    }

    public TimeSpan DownTime
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => Config.DownTime;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => Config.DownTime = value;
    }

    // resets the configuration settings to their default values.
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Reset()
    {
        Config.Reset();
    }
}