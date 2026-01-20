//using BO;
using System.Runtime.CompilerServices;

namespace Helpers;

/// <summary>
/// Internal BL manager for all Application's Configuration Variables and Clock logic policies
/// </summary>
internal static class AdminManager //stage 4
{
    #region Stage 4-7
    private static readonly DalApi.IDal s_dal = DalApi.Factory.Get; //stage 4
    
    /// <summary>
    /// Property for providing current application's clock value for any BL class that may need it
    /// </summary>
    internal static DateTime Now { get => s_dal.Config.Clock; } //stage 4

    internal static event Action? ConfigUpdatedObservers; //stage 5 - for config update observers
    internal static event Action? ClockUpdatedObservers; //stage 5 - for clock update observers

    /// <summary>
    /// Method to update application's clock from any BL class as may be required
    /// </summary>
    /// <param name="newClock">updated clock value</param>
    internal static void UpdateClock(DateTime newClock) //stage 4-7
    {
        var oldClock = s_dal.Config.Clock; //stage 4
        s_dal.Config.Clock = newClock; //stage 4

        //Add calls here to any logic method that should be called periodically,
        //after each clock update
        //for example, Periodic students' updates:
        // - Go through all students to update properties that are affected by the clock update
        // - (students become not active after 5 years etc.)

        //TO_DO: //stage 4
        // CourierManager.PeriodicCouriersUpdates(oldClock, newClock); //stage 4. to be removed in stage 7 and replaced as below
        // DeliveryManager.PeriodicDeliveriesUpdates(oldClock, newClock); //stage 4. to be removed in stage 7 and replaced as below
        //OrderManager.PeriodicOrdersUpdates(oldClock, newClock); //stage 4. to be removed in stage 7 and replaced as below
        //OrderManager.PeriodicAutoAssignPendingOrders(oldClock, newClock); //stage 4. to be removed in stage 7 and replaced as below
        //...

        //TO_DO: //stage 7
        //if (_periodicTask is null || _periodicTask.IsCompleted) //stage 7
        _ = Task.Run(() => CourierManager.PeriodicCouriersUpdates(oldClock, newClock));
        _ = Task.Run(() => DeliveryManager.PeriodicDeliveriesUpdates(oldClock, newClock));
        _ = Task.Run(() => OrderManager.PeriodicOrdersUpdates(oldClock, newClock));
        _ =  Task.Run(() => OrderManager.PeriodicAutoAssignPendingOrders(oldClock, newClock));


        //Calling all the observers of clock update
        ClockUpdatedObservers?.Invoke(); //prepared for stage 5
    }

    /// <summary>
    /// Method for providing current configuration variables values for any BL class that may need it
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)] // its like locking this function 
    internal static BO.Config GetConfig() //stage 4
    => new BO.Config()
    {
        Clock = s_dal.Config.Clock,
        AdminId = s_dal.Config.AdminId,
        CompanyName = s_dal.Config.CompanyName,
        Longitude = s_dal.Config.Longitude,
        Latitude = s_dal.Config.Latitude,
        MaxDist = s_dal.Config.MaxDist,
        AvgCarMPH = s_dal.Config.AvgCarMPH,
        AvgMotorcycleMPH = s_dal.Config.AvgMotorcycleMPH,
        AvgBicycleMPH = s_dal.Config.AvgBicycleMPH,
        AvgWalkMPH = s_dal.Config.AvgWalkMPH,
        MaxDelTime = s_dal.Config.MaxDelTime,
        RiskRange = s_dal.Config.RiskRange,
        DownTime = s_dal.Config.DownTime,
    };

    /// <summary>
    /// Method for setting current configuration variables values for any BL class that may need it
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    internal static void SetConfig(BO.Config configuration) //stage 4
    {
        bool configChanged = false; // stage 5

        if (s_dal.Config.Clock != configuration.Clock) //stage 4
        {
            s_dal.Config.Clock = configuration.Clock;
            configChanged = true;
        }
        if (s_dal.Config.AdminId != configuration.AdminId) //stage 4
        {
            s_dal.Config.AdminId = configuration.AdminId;
            configChanged = true;
        }
        if (s_dal.Config.CompanyName != configuration.CompanyName) //stage 4
        {
            s_dal.Config.CompanyName = configuration.CompanyName;
            configChanged = true;
        }
        if (s_dal.Config.Longitude != configuration.Longitude) //stage 4
        {
            s_dal.Config.Longitude = configuration.Longitude;
            configChanged = true;
        }
        if (s_dal.Config.Latitude != configuration.Latitude) //stage 4
        {
            s_dal.Config.Latitude = configuration.Latitude;
            configChanged = true;
        }
        if (s_dal.Config.MaxDist != configuration.MaxDist) //stage 4
        {
            s_dal.Config.MaxDist = configuration.MaxDist;
            configChanged = true;
        }
        if (s_dal.Config.AvgCarMPH != configuration.AvgCarMPH) //stage 4
        {
            s_dal.Config.AvgCarMPH = configuration.AvgCarMPH;
            configChanged = true;
        }
        if (s_dal.Config.AvgMotorcycleMPH != configuration.AvgMotorcycleMPH) //stage 4
        {
            s_dal.Config.AvgMotorcycleMPH = configuration.AvgMotorcycleMPH;
            configChanged = true;
        }
        if (s_dal.Config.AvgBicycleMPH != configuration.AvgBicycleMPH) //stage 4
        {
            s_dal.Config.AvgBicycleMPH = configuration.AvgBicycleMPH;
            configChanged = true;
        }
        if (s_dal.Config.AvgWalkMPH != configuration.AvgWalkMPH) //stage 4
        {
            s_dal.Config.AvgWalkMPH = configuration.AvgWalkMPH;
            configChanged = true;
        }
        if (s_dal.Config.MaxDelTime != configuration.MaxDelTime) //stage 4
        {
            s_dal.Config.MaxDelTime = configuration.MaxDelTime;
            configChanged = true;
        }
        if (s_dal.Config.RiskRange != configuration.RiskRange) //stage 4
        {
            s_dal.Config.RiskRange = configuration.RiskRange;
            configChanged = true;
        }
        if (s_dal.Config.DownTime != configuration.DownTime) //stage 4
        {
            s_dal.Config.DownTime = configuration.DownTime;
            configChanged = true;
        }
        
        if (configChanged) // stage 5
            ConfigUpdatedObservers?.Invoke(); // stage 5
    }

    /// <summary>
    /// Resets the application database to its initial state and notifies observers of configuration updates.
    /// </summary>
    /// <remarks>This method is intended for internal use and should be called only when a complete reset of
    /// the database and related configuration is required. All registered configuration update observers are notified
    /// after the reset completes. This operation is not thread-safe beyond the internal locking and may have
    /// significant side effects, including loss of all current data.</remarks>
    internal static void ResetDB() //stage 4-7
    {
        lock (BlMutex) //stage 7
        {
            s_dal.ResetDB(); //stage 4
            AdminManager.UpdateClock(AdminManager.Now); //stage 5 - needed since we want the label on Pl to be updated
            //AdminManager.SetConfig(AdminManager.GetConfig()); //stage 5 - needed to update PL 
            ConfigUpdatedObservers?.Invoke();
        }
    }

    /// <summary>
    /// Initializes the database and updates related administrative state in a thread-safe manner.
    /// </summary>
    /// <remarks>This method acquires a lock to ensure that database initialization and related updates are
    /// performed atomically. It also notifies observers of configuration updates. This method is intended for internal
    /// use and should not be called directly by external code.</remarks>
    internal static void InitializeDB() //stage 4-7
    {
        lock (BlMutex) //stage 7
        {
            DalTest.Initialization.Do(); //stage 4
            AdminManager.UpdateClock(AdminManager.Now);  //stage 5 - needed since we want the label on Pl to be updated           
            //AdminManager.SetConfig(AdminManager.GetConfig()); //stage 5 - needed for update the PL
            ConfigUpdatedObservers?.Invoke(); 
        }
    }

    #endregion Stage 4-7

    #region Stage 7 base

    /// <summary>    
    /// Mutex to use from BL methods to get mutual exclusion while the simulator is running
    /// </summary>
    internal static readonly object BlMutex = new(); // BlMutex = s_dal; // This field is actually the same as s_dal - it is defined for readability of locks
    /// <summary>
    /// The thread of the simulator
    /// </summary>
    private static volatile Thread? s_thread;
    /// <summary>
    /// The Interval for clock updating
    /// in minutes by second (default value is 1, will be set on Start())    
    /// </summary>
    private static int s_interval = 1;
    /// <summary>
    /// The flag that signs whether simulator is running
    /// 
    private static volatile bool s_stop = false;


    /// <summary>
    /// Throws an exception if the simulator is currently running.
    /// </summary>
    /// <exception cref="BO.BLTemporaryNotAvailableException">Thrown if the simulator is running, indicating that the requested operation cannot be performed at this time.</exception>
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7                                                 
    public static void ThrowOnSimulatorIsRunning()
    {
        if (s_thread is not null)
            throw new BO.BLTemporaryNotAvailableException("Cannot perform the operation since Simulator is running");
    }


    /// <summary>
    /// Starts the background clock runner thread if it is not already running.
    /// </summary>
    /// <remarks>If the clock runner thread is already running, this method has no effect. This method is
    /// thread-safe.</remarks>
    /// <param name="interval">The interval, in milliseconds, at which the clock runner operates. Must be a positive integer.</param>
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7                                                 
    internal static void Start(int interval)
    {
        if (s_thread is null)
        {
            s_interval = interval;
            s_stop = false;
            s_thread = new(clockRunner) { Name = "ClockRunner" };
            s_thread.Start();
        }
    }


    /// <summary>
    /// Stops the background clock runner thread if it is currently running.
    /// </summary>
    /// <remarks>This method is thread-safe and has no effect if the clock runner thread is not active. After
    /// calling this method, the clock runner cannot be restarted unless reinitialized by other means.</remarks>
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7                                                 
    internal static void Stop()
    {
        if (s_thread is not null)
        {
            s_stop = true;
            s_thread.Interrupt(); //awake a sleeping thread
            s_thread.Name = "ClockRunner stopped";
            s_thread = null;
        }
    }

    /// <summary>
    /// Advances the system clock and triggers periodic updates for couriers, deliveries, and orders while the service
    /// is running.
    /// </summary>
    /// <remarks>This method runs in a loop, updating the simulated clock and initiating background tasks to
    /// process time-based updates for couriers, deliveries, and orders. It is intended to be called as part of the
    /// application's background processing and will continue running until the stop condition is signaled. The method
    /// is not intended to be called directly by user code.</remarks>
    private static void clockRunner()
    {
        while (!s_stop)
        {
            UpdateClock(Now.AddMinutes(s_interval));
            //TO_DO: //stage 7
            //Add calls here to any logic simulation that was required in stage 7
            //for example: course registration simulation
            //etc…
            _ = Task.Run(() => CourierManager.PeriodicCouriersUpdates(Now.AddMinutes(-s_interval), Now));
            _ = Task.Run(() => DeliveryManager.PeriodicDeliveriesUpdates(Now.AddMinutes(-s_interval), Now));
            _ = Task.Run(() => OrderManager.PeriodicOrdersUpdates(Now.AddMinutes(-s_interval), Now));
            _ = Task.Run(() => CourierManager.SimulateCourierActivity());
            try
            {
                Thread.Sleep(1000); // 1 second
           	}

        catch (ThreadInterruptedException) { }
        }
    }


    #endregion Stage 7 base
}
