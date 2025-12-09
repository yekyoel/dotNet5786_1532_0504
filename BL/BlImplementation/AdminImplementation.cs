using BlApi;
using Helpers;

namespace BlImplementation;

internal class AdminImplementation : IAdmin
{
    public System.DateTime ForwardClock(BO.Time forward)
    {
        var now = AdminManager.Now;
        System.DateTime newClock = forward switch
        {
            BO.Time.Minute => now.AddMinutes(1),
            BO.Time.Hour => now.AddHours(1),
            BO.Time.Day => now.AddDays(1),
            BO.Time.Month => now.AddMonths(1),
            BO.Time.Year => now.AddYears(1),
            _ => throw new System.ArgumentOutOfRangeException(nameof(forward), "Unsupported time unit")
        };

        AdminManager.UpdateClock(newClock);
        return newClock;
    }

    public System.DateTime GetClock()
    {
        return AdminManager.Now;
    }

    public BO.Config GetConfig()
    {
        return AdminManager.GetConfig();
    }

    public void InitializeDB()
    {
        AdminManager.InitializeDB();
    }

    public void ResetDB()
    {
        AdminManager.ResetDB();
    }

    public void SetConfig(BO.Config config)
    {
        if (config is null) throw new System.ArgumentNullException(nameof(config));
        AdminManager.SetConfig(config);
    }
}
