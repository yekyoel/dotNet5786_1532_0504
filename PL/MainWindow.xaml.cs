using PL.Courier;
using PL.Helpers;
using PL.Order;
    using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PL;

/// <summary>
/// Interaction logic for MainWindow.xamlonfig
/// </summary>
public partial class MainWindow : Window
{
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
    private readonly ObserverMutex _clockMutex = new(); //stage 7


    public MainWindow()
    {
        InitializeComponent();
    }

    #region Dependency Properties for Config Bindings

    public static readonly DependencyProperty ConfigurationProperty =
        DependencyProperty.Register("Configuration", typeof(BO.Config), typeof(MainWindow));

    /// <summary>
    /// Gets or sets the configuration settings for the control.
    /// </summary>
    public BO.Config Configuration
    {
        get { return (BO.Config)GetValue(ConfigurationProperty); }
        set { SetValue(ConfigurationProperty, value); }
    }

    public static readonly DependencyProperty CurrentTimeProperty =
        DependencyProperty.Register("CurrentTime", typeof(DateTime), typeof(MainWindow));

    /// <summary>
    /// Gets or sets the current time value represented by the control.
    /// </summary>
    public DateTime CurrentTime
    {
        get { return (DateTime)GetValue(CurrentTimeProperty); }
        set { SetValue(CurrentTimeProperty, value); }
    }

    // Order summary text properties (for buttons)
    public static readonly DependencyProperty OpenOnTimeTextProperty =
        DependencyProperty.Register("OpenOnTimeText", typeof(string), typeof(MainWindow));

    public string OpenOnTimeText
    {
        get => (string)GetValue(OpenOnTimeTextProperty);
        set => SetValue(OpenOnTimeTextProperty, value);
    }

    public static readonly DependencyProperty OpenInRiskTextProperty =
        DependencyProperty.Register("OpenInRiskText", typeof(string), typeof(MainWindow));

    public string OpenInRiskText
    {
        get => (string)GetValue(OpenInRiskTextProperty);
        set => SetValue(OpenInRiskTextProperty, value);
    }

    public static readonly DependencyProperty OpenLateTextProperty =
        DependencyProperty.Register("OpenLateText", typeof(string), typeof(MainWindow));

    public string OpenLateText
    {
        get => (string)GetValue(OpenLateTextProperty);
        set => SetValue(OpenLateTextProperty, value);
    }

    public static readonly DependencyProperty CompletedOnTimeTextProperty =
        DependencyProperty.Register("CompletedOnTimeText", typeof(string), typeof(MainWindow));

    public string CompletedOnTimeText
    {
        get => (string)GetValue(CompletedOnTimeTextProperty);
        set => SetValue(CompletedOnTimeTextProperty, value);
    }

    public static readonly DependencyProperty CompletedLateTextProperty =
        DependencyProperty.Register("CompletedLateText", typeof(string), typeof(MainWindow));

    public string CompletedLateText
    {
        get => (string)GetValue(CompletedLateTextProperty);
        set => SetValue(CompletedLateTextProperty, value);
    }

    public static readonly DependencyProperty IntervalProperty = 
        DependencyProperty.Register("Interval", typeof(int), typeof(MainWindow), new PropertyMetadata(1));
        
    public int Interval
    {
        get => (int)GetValue(IntervalProperty);
        set => SetValue(IntervalProperty, value);
    }

    public static readonly DependencyProperty SimulatorStatusProperty =
       DependencyProperty.Register("SimulatorStatus", typeof(bool), typeof(MainWindow), new PropertyMetadata(false, OnSimulatorStatusChanged));

    public bool SimulatorStatus
    {
        get => (bool)GetValue(SimulatorStatusProperty);
        set => SetValue(SimulatorStatusProperty, value);
    }

    private static void OnSimulatorStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MainWindow window)
        {
            // Sync dependent properties when SimulatorStatus changes
            window.IsSimulatorStopped = !window.SimulatorStatus;
            window.SimulatorButtonText = window.SimulatorStatus ? "Stop Simulator" : "Start Simulator";
        }
    }

    public static readonly DependencyProperty IsSimulatorStoppedProperty =
        DependencyProperty.Register("IsSimulatorStopped", typeof(bool), typeof(MainWindow), new PropertyMetadata(true));

    public bool IsSimulatorStopped
    {
        get => (bool)GetValue(IsSimulatorStoppedProperty);
        set => SetValue(IsSimulatorStoppedProperty, value);
    }

    public static readonly DependencyProperty SimulatorButtonTextProperty =
        DependencyProperty.Register("SimulatorButtonText", typeof(string), typeof(MainWindow), new PropertyMetadata("Start Simulator"));

    public string SimulatorButtonText
    {
        get => (string)GetValue(SimulatorButtonTextProperty);
        set => SetValue(SimulatorButtonTextProperty, value);
    }

    #endregion


    #region Observers and Window Events

    /// <summary>
    /// Updates the current time by retrieving the latest clock value from the administration service.
    /// </summary>
    private void ClockObserver()
    {
        if(_clockMutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;
        Dispatcher.BeginInvoke(async () =>
        {
            CurrentTime = s_bl.Admin.GetClock();
            LoadOrderSummary();

            if (await _clockMutex.UnsetLoadInProgressAndCheckRestartRequested())
                ClockObserver();

        });
    }

    /// <summary>
    /// Initializes or updates the configuration by retrieving the latest settings from the administration service.
    /// </summary>
    private void ConfigObserver()
    {
        if (_clockMutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;
        Dispatcher.BeginInvoke(async () =>
        {
            Configuration = s_bl.Admin.GetConfig();
            LoadOrderSummary();

            if (await _clockMutex.UnsetLoadInProgressAndCheckRestartRequested())
                ConfigObserver();

        });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            CurrentTime = s_bl.Admin.GetClock();
           // Configuration = s_bl.Admin.GetConfig();

            s_bl.Admin.AddClockObserver(ClockObserver);

            s_bl.Admin.AddConfigObserver(ConfigObserver);

            LoadOrderSummary();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error initializing Order list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Handles the window's Closed event by unsubscribing observers from administrative notifications.
    /// </summary>
    /// <remarks>This method ensures that observers are properly unsubscribed when the window is closed to
    /// prevent memory leaks and unnecessary event handling. Exceptions during unsubscription are caught and logged to
    /// avoid disrupting the window closing process.</remarks>
    /// <param name="sender">The source of the event, typically the window being closed.</param>
    /// <param name="e">An object that contains the event data.</param>
    private void Window_Closed(object sender, EventArgs e)
    {
        try
        {
            if (SimulatorStatus)
            {
                s_bl.Admin.StopSimulator();
            }

            s_bl.Admin.RemoveClockObserver(ClockObserver);
            s_bl.Admin.RemoveConfigObserver(ConfigObserver);

        }
        catch (Exception ex)
        {
            // Log exception but don't crash on window close
            System.Diagnostics.Debug.WriteLine($"Error while unsubscribing from order updates: {ex.Message}");
        }
    }

    #endregion



    /// <summary>
    /// Validates the specified configuration object and throws an exception if any required property is invalid.
    /// </summary>
    /// <param name="config">The configuration object to validate. All required properties must be set to valid values.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if any numeric or time span property of the configuration is outside its valid range. For example, if an
    /// ID is not positive, a speed is not greater than zero, or a time span is negative.</exception>
    /// <exception cref="ArgumentException">Thrown if a required string property, such as the company name, is null, empty, or consists only of white-space
    /// characters.</exception>
    private static void ValidateConfigOrThrow(BO.Config config)
    {
        // AdminId
        if (config.AdminId <= 0)
            throw new ArgumentOutOfRangeException(nameof(config.AdminId), "Admin ID must be a positive integer.");

        // CompanyName
        if (string.IsNullOrWhiteSpace(config.CompanyName))
            throw new ArgumentException("Company Name is required.", nameof(config.CompanyName));

        // Speeds
        if (config.AvgCarMPH <= 0)
            throw new ArgumentOutOfRangeException(nameof(config.AvgCarMPH), "Avg Car MPH must be greater than 0.");

        if (config.AvgMotorcycleMPH <= 0)
            throw new ArgumentOutOfRangeException(nameof(config.AvgMotorcycleMPH), "Avg Motorcycle MPH must be greater than 0.");

        if (config.AvgBicycleMPH <= 0)
            throw new ArgumentOutOfRangeException(nameof(config.AvgBicycleMPH), "Avg Bicycle MPH must be greater than 0.");

        if (config.AvgWalkMPH <= 0)
            throw new ArgumentOutOfRangeException(nameof(config.AvgWalkMPH), "Avg Walk MPH must be greater than 0.");

        // Time spans
        if (config.MaxDelTime <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(config.MaxDelTime), "Max Delivery Time must be greater than 00:00:00.");

        if (config.RiskRange < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(config.RiskRange), "Risk Range cannot be negative.");

        if (config.DownTime < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(config.DownTime), "Down Time cannot be negative.");

    }


    /// <summary>
    /// Loads order summary counts from BL.StatusTotal and updates summary texts.
    /// </summary>
    private void LoadOrderSummary()
    {
        try
        {
            int adminId = s_bl.Admin.GetConfig().AdminId;
            int[] totals = s_bl.Order.StatusTotal(adminId);

            int orderStatusCount = Enum.GetValues(typeof(BO.OrderStatus)).Length;
            int scheduleStatusCount = Enum.GetValues(typeof(BO.ScheduleStatus)).Length;

            int Index(BO.OrderStatus os, BO.ScheduleStatus ss) =>
                (int)os * scheduleStatusCount + (int)ss;

            int openOnTime = totals[Index(BO.OrderStatus.Open, BO.ScheduleStatus.OnTime)];
            int openInRisk = totals[Index(BO.OrderStatus.Open, BO.ScheduleStatus.InRisk)];
            int openLate = totals[Index(BO.OrderStatus.Open, BO.ScheduleStatus.Late)];
            int completedOnTime = totals[Index(BO.OrderStatus.Completed, BO.ScheduleStatus.OnTime)];
            int completedLate = totals[Index(BO.OrderStatus.Completed, BO.ScheduleStatus.Late)];

            OpenOnTimeText = $"Open / OnTime: {openOnTime}";
            OpenInRiskText = $"Open / InRisk: {openInRisk}";
            OpenLateText = $"Open / Late: {openLate}";
            CompletedOnTimeText = $"Completed / OnTime: {completedOnTime}";
            CompletedLateText = $"Completed / Late: {completedLate}";
        }
        catch
        {
            OpenOnTimeText = "Open / OnTime: -";
            OpenInRiskText = "Open / InRisk: -";
            OpenLateText = "Open / Late: -";
            CompletedOnTimeText = "Completed / OnTime: -";
            CompletedLateText = "Completed / Late: -";
        }
    }



    #region buttons for clock manipulation and simulation

    /// <summary>
    /// Handles the Click event of the Add One Month button, advancing the application clock by one month.
    /// </summary>
    /// <remarks>This method updates the application's current time by forwarding the internal clock by one
    /// month. If an error occurs during the update, an error message is displayed to the user.</remarks>
    /// <param name="sender">The source of the event, typically the button that was clicked.</param>
    /// <param name="e">The event data associated with the Click event.</param>
    private void btnAddOneMon_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            s_bl.Admin.ForwardClock(BO.Time.Month);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error updating clock: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Handles the Click event of the Add One Minute button by advancing the application clock by one minute.
    /// </summary>
    /// <remarks>If an error occurs while updating the clock, an error message is displayed to the
    /// user.</remarks>
    /// <param name="sender">The source of the event, typically the button that was clicked.</param>
    /// <param name="e">The event data associated with the Click event.</param>
    private void btnAddOneMin_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            s_bl.Admin.ForwardClock(BO.Time.Minute);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error updating clock: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Handles the Click event of the Add One Hour button by advancing the application clock by one hour.
    /// </summary>
    /// <remarks>If an error occurs while updating the clock, an error message is displayed to the
    /// user.</remarks>
    /// <param name="sender">The source of the event, typically the button that was clicked.</param>
    /// <param name="e">The event data associated with the Click event.</param>
    private void btnAddOneHr_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            s_bl.Admin.ForwardClock(BO.Time.Hour);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error updating clock: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Handles the Click event of the Add One day button by advancing the application clock by one hour.
    /// </summary>
    /// <remarks>If an error occurs while updating the clock, an error message is displayed to the
    /// user.</remarks>
    /// <param name="sender">The source of the event, typically the button that was clicked.</param>
    /// <param name="e">The event data associated with the Click event.</param>
    private void btnAddOneDay_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            s_bl.Admin.ForwardClock(BO.Time.Day);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error updating clock: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Handles the Click event of the Add One year button by advancing the application clock by one hour.
    /// </summary>
    /// <remarks>If an error occurs while updating the clock, an error message is displayed to the
    /// user.</remarks>
    /// <param name="sender">The source of the event, typically the button that was clicked.</param>
    /// <param name="e">The event data associated with the Click event.</param>
    private void btnAddOneYr_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            s_bl.Admin.ForwardClock(BO.Time.Year);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error updating clock: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnSimStatus_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if(!SimulatorStatus) // If we are about to start (was false)
            {
                s_bl.Admin.StartSimulator(Interval);
                SimulatorStatus = true; // Updates UI via callback
            }
            else // If we are about to stop (was true)
            {
                s_bl.Admin.StopSimulator();
                SimulatorStatus = false; // Updates UI via callback to re-enable controls
            }

        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error starting/stopping simulator: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion


    #region database buttons

    /// <summary>
    /// Handles the Click event of the Update button to validate and save the current configuration settings.
    /// </summary>
    /// <param name="sender">The source of the event, typically the Update button.</param>
    /// <param name="e">The event data associated with the Click event.</param>
    private void btnUpdateObj_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (Configuration is null)
                Configuration = new BO.Config();

            ValidateConfigOrThrow(Configuration);
            s_bl.Admin.SetConfig(Configuration);

            MessageBox.Show("Configuration saved successfully!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Invalid Configuration",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Initializes the database with test data and reloads all UI properties.
    /// Shows hourglass cursor during the operation.
    /// </summary>
    private void btnInitialize_Click(object sender, RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show("Are you sure you want to initialize the database?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                // Show loading cursor (hourglass)
                this.Cursor = System.Windows.Input.Cursors.Wait;

                s_bl.Admin.InitializeDB();
                Configuration = s_bl.Admin.GetConfig();

                MessageBox.Show("Database initialized successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Restore normal cursor
                this.Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }
    }

    /// <summary>
    /// Resets the database by clearing all data.
    /// Shows hourglass cursor during the operation.
    /// </summary>
    private void btnResetDB_Click(object sender, RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show("Are you sure you want to reset the database?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                // Show loading cursor (hourglass)
                this.Cursor = System.Windows.Input.Cursors.Wait;

                s_bl.Admin.ResetDB();
                Configuration = null;

                MessageBox.Show("Database reset successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error resetting database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Restore normal cursor
                this.Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }
    }

    /// <summary>
    /// Handles the Click event of the Courier List button by opening the Courier List window.
    /// </summary>
    /// <param name="sender">The source of the event, typically the button that was clicked.</param>
    /// <param name="e">The event data associated with the Click event.</param>
    private void btnCourierListDisplay_Click(object sender, RoutedEventArgs e)
    {
        new CourierListWindow().Show();
    }

    /// <summary>
    /// Handles the Click event of the Order List button by displaying the Order List window.
    /// </summary>
    /// <param name="sender">The source of the event, typically the Order List button.</param>
    /// <param name="e">The event data associated with the Click event.</param>
    private void btnOrderListDisplay_Click(object sender, RoutedEventArgs e)
    {
        new OrderListWindow().Show();
    }

    #endregion

    #region order summary buttons

    /// <summary>
    /// Handles the Click event of the Open On Time button by displaying a window with orders that have an open status.
    /// </summary>
    /// <param name="sender">The source of the event, typically the button that was clicked.</param>
    /// <param name="e">The event data associated with the Click event.</param>
    private void BtnOpenOnTime_Click(object sender, RoutedEventArgs e)
    {
        var w = new OrderListWindow { FilterStatus = BO.OrderStatus.Open, FilterScheduleStatus = BO.ScheduleStatus.OnTime };
        w.Show();
    }

    /// <summary>
    /// Handles the Click event of the Open in risk button by displaying a window with orders that have an open status.
    /// </summary>
    /// <param name="sender">The source of the event, typically the button that was clicked.</param>
    /// <param name="e">The event data associated with the Click event.</param>
    private void BtnOpenInRisk_Click(object sender, RoutedEventArgs e)
    {
        var w = new OrderListWindow { FilterStatus = BO.OrderStatus.Open, FilterScheduleStatus = BO.ScheduleStatus.InRisk };
        w.Show();
    }

    /// <summary>
    /// Handles the Click event of the button to display a window listing orders with the status set to Open.
    /// </summary>
    /// <param name="sender">The source of the event, typically the button that was clicked.</param>
    /// <param name="e">The event data associated with the Click event.</param>
    private void BtnOpenLate_Click(object sender, RoutedEventArgs e)
    {
        var w = new OrderListWindow { FilterStatus = BO.OrderStatus.Open, FilterScheduleStatus = BO.ScheduleStatus.Late };
        w.Show();
    }

    /// <summary>
    /// Handles the Click event for the Completed On Time button, displaying a window with orders filtered by completed
    /// status.
    /// </summary>
    /// <param name="sender">The source of the event, typically the button that was clicked.</param>
    /// <param name="e">The event data associated with the Click event.</param>
    private void BtnCompletedOnTime_Click(object sender, RoutedEventArgs e)
    {
        var w = new OrderListWindow { FilterStatus = BO.OrderStatus.Completed, FilterScheduleStatus = BO.ScheduleStatus.OnTime };
        w.Show();
    }

    /// <summary>
    /// Handles the Click event for the Completed Late button, displaying a window with orders filtered by completed
    /// status.
    /// </summary>
    /// <param name="sender">The source of the event, typically the button that was clicked.</param>
    /// <param name="e">The event data associated with the Click event.</param>
    private void BtnCompletedLate_Click(object sender, RoutedEventArgs e)
    {
        var w = new OrderListWindow { FilterStatus = BO.OrderStatus.Completed, FilterScheduleStatus = BO.ScheduleStatus.Late };
        w.Show();
    }

    #endregion
}
