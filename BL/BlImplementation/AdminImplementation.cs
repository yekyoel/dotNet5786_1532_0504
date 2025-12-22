using BlApi;
using Helpers;

namespace BlImplementation;

internal class AdminImplementation : IAdmin
{
    /// <summary>
    /// Advances the system clock by the specified time unit and returns the updated date and time.
    /// </summary>
    /// <remarks>This method updates the system clock managed by AdminManager. The change is immediately
    /// applied and affects all subsequent operations that rely on the current time.</remarks>
    /// <param name="forward">The time unit by which to advance the clock. Must be one of Minute, Hour, Day, Month, or Year.</param>
    /// <returns>A DateTime value representing the new system clock after it has been advanced.</returns>
    /// <exception cref="System.ArgumentOutOfRangeException">Thrown if the specified time unit is not supported.</exception>
    public System.DateTime ForwardClock(BO.Time forward)
    {
        var now = AdminManager.Now;
        System.DateTime newClock = forward switch
        {
            BO.Time.Second => now.AddSeconds(1),
            BO.Time.Minute => now.AddMinutes(1),
            BO.Time.Hour => now.AddHours(1),
            BO.Time.Day => now.AddDays(1),
            BO.Time.Year => now.AddYears(1),
            _ => throw new System.ArgumentOutOfRangeException(nameof(forward), "Unsupported time unit") // in case of an invalid enum value
        };

        AdminManager.UpdateClock(newClock);
        return newClock;
    }

    /// <summary>
    /// Gets the current date and time according to the system clock.
    /// </summary>
    /// <returns>A <see cref="System.DateTime"/> value representing the current local date and time.</returns>
    public System.DateTime GetClock()
    {
        return AdminManager.Now;
    }

    /// <summary>
    /// Retrieves the current application configuration settings.
    /// </summary>
    /// <returns>A <see cref="BO.Config"/> object containing the application's configuration values.</returns>
    public BO.Config GetConfig()
    {
        return AdminManager.GetConfig();
    }

    /// <summary>
    /// Initializes the database to ensure it is ready for use.
    /// </summary>
    /// <remarks>Call this method before performing any operations that require access to the database. This
    /// method is typically used during application startup to set up required database structures or state.</remarks>
    public void InitializeDB()
    {
        AdminManager.InitializeDB();
    }

    /// <summary>
    /// Resets the database to its initial state, removing all data and restoring default settings.
    /// </summary>
    /// <remarks>Use this method with caution, as all existing data will be permanently deleted and cannot be
    /// recovered. This operation is typically intended for administrative or testing purposes.</remarks>
    public void ResetDB()
    {
        AdminManager.ResetDB();
    }

    /// <summary>
    /// Sets the application configuration using the specified configuration object.
    /// </summary>
    /// <param name="config">The configuration settings to apply. Cannot be null.</param>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="config"/> is null.</exception>
    public void SetConfig(BO.Config config)
    {
        if (config is null) throw new System.ArgumentNullException(nameof(config));
        AdminManager.SetConfig(config);
    }

    public void AddClockObserver(Action clockObserver) =>
    AdminManager.ClockUpdatedObservers += clockObserver;
    public void RemoveClockObserver(Action clockObserver) =>
    AdminManager.ClockUpdatedObservers -= clockObserver;
    public void AddConfigObserver(Action configObserver) =>
   AdminManager.ConfigUpdatedObservers += configObserver;
    public void RemoveConfigObserver(Action configObserver) =>
    AdminManager.ConfigUpdatedObservers -= configObserver;
}
