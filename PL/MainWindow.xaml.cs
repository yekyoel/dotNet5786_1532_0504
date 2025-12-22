using PL.Courier;
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
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
    private readonly Action _clockObserver;
    private readonly Action _configObserver;

    public MainWindow()
    {
        InitializeComponent();

        // create observer delegates once so we can remove them on close
        _clockObserver = OnClockUpdated;
        _configObserver = OnConfigUpdated;

        // subscribe to BL clock updates so UI refreshes automatically
        s_bl.Admin.AddClockObserver(_clockObserver);
        
        // subscribe to BL config updates
        s_bl.Admin.AddConfigObserver(_configObserver);

        // ensure we unsubscribe when window closes
        this.Closed += (_, _) => 
        {
            s_bl.Admin.RemoveClockObserver(_clockObserver);
            s_bl.Admin.RemoveConfigObserver(_configObserver);
        };
    }

    #region Dependency Properties for Config Bindings

    public static readonly DependencyProperty ConfigurationProperty =
        DependencyProperty.Register("Configuration", typeof(BO.Config), typeof(MainWindow));

    public BO.Config Configuration
    {
        get { return (BO.Config)GetValue(ConfigurationProperty); }
        set { SetValue(ConfigurationProperty, value); }
    }

    public static readonly DependencyProperty CurrentTimeProperty =
        DependencyProperty.Register("CurrentTime", typeof(DateTime), typeof(MainWindow));

    public DateTime CurrentTime
    {
        get { return (DateTime)GetValue(CurrentTimeProperty); }
        set { SetValue(CurrentTimeProperty, value); }
    }

    public static readonly DependencyProperty AdminIdProperty =
        DependencyProperty.Register("AdminId", typeof(int), typeof(MainWindow));

    public int AdminId
    {
        get { return (int)GetValue(AdminIdProperty); }
        set { SetValue(AdminIdProperty, value); }
    }

    public static readonly DependencyProperty CompanyNameProperty =
        DependencyProperty.Register("CompanyName", typeof(string), typeof(MainWindow));

    public string CompanyName
    {
        get { return (string)GetValue(CompanyNameProperty); }
        set { SetValue(CompanyNameProperty, value); }
    }

    public static readonly DependencyProperty MaxDistanceProperty =
        DependencyProperty.Register("MaxDist", typeof(double), typeof(MainWindow));

    public double MaxDist
    {
        get { return (double)GetValue(MaxDistanceProperty); }
        set { SetValue(MaxDistanceProperty, value); }
    }

    public static readonly DependencyProperty AvgCarMPHProperty =
        DependencyProperty.Register("AvgCarMPH", typeof(double), typeof(MainWindow));

    public double AvgCarMPH
    {
        get { return (double)GetValue(AvgCarMPHProperty); }
        set { SetValue(AvgCarMPHProperty, value); }
    }

    public static readonly DependencyProperty AvgMotorcycleMPHProperty =
        DependencyProperty.Register("AvgMotorcycleMPH", typeof(double), typeof(MainWindow));

    public double AvgMotorcycleMPH
    {
        get { return (double)GetValue(AvgMotorcycleMPHProperty); }
        set { SetValue(AvgMotorcycleMPHProperty, value); }
    }

    public static readonly DependencyProperty AvgBicycleMPHProperty =
        DependencyProperty.Register("AvgBikeMPH", typeof(double), typeof(MainWindow));

    public double AvgBikeMPH
    {
        get { return (double)GetValue(AvgBicycleMPHProperty); }
        set { SetValue(AvgBicycleMPHProperty, value); }
    }

    public static readonly DependencyProperty AvgWalkMPHProperty =
        DependencyProperty.Register("AvgWalkMPH", typeof(double), typeof(MainWindow));

    public double AvgWalkMPH
    {
        get { return (double)GetValue(AvgWalkMPHProperty); }
        set { SetValue(AvgWalkMPHProperty, value); }
    }

    public static readonly DependencyProperty MaxDeliveryTimeProperty =
        DependencyProperty.Register("MaxDeliveryTime", typeof(TimeSpan), typeof(MainWindow));

    public TimeSpan MaxDeliveryTime
    {
        get { return (TimeSpan)GetValue(MaxDeliveryTimeProperty); }
        set { SetValue(MaxDeliveryTimeProperty, value); }
    }

    public static readonly DependencyProperty RiskRangeProperty =
        DependencyProperty.Register("RiskRange", typeof(TimeSpan), typeof(MainWindow));

    public TimeSpan RiskRange
    {
        get { return (TimeSpan)GetValue(RiskRangeProperty); }
        set { SetValue(RiskRangeProperty, value); }
    }

    public static readonly DependencyProperty DownTimeProperty =
        DependencyProperty.Register("DownTime", typeof(TimeSpan), typeof(MainWindow));

    public TimeSpan DownTime
    {
        get { return (TimeSpan)GetValue(DownTimeProperty); }
        set { SetValue(DownTimeProperty, value); }
    }

    #endregion

    // update config button handler
    private void btnUpdateObj_Click(object sender, RoutedEventArgs e)
    {
        var config = new BO.Config
        {
            AdminId = AdminId,
            CompanyName = CompanyName,
            MaxDist = MaxDist,
            AvgCarMPH = AvgCarMPH,
            AvgMotorcycleMPH = AvgMotorcycleMPH,
            AvgBicycleMPH = AvgBikeMPH,
            AvgWalkMPH = AvgWalkMPH,
            MaxDelTime = MaxDeliveryTime,
            RiskRange = RiskRange,
            DownTime = DownTime
        };

        s_bl.Admin.SetConfig(config);
    }


    private void OnClockUpdated()
    {
        // run on UI thread and fetch current clock from BL
        Dispatcher.Invoke(() => CurrentTime = s_bl.Admin.GetClock());
    }

    /// <summary>
    /// Called when configuration is updated in the BL layer.
    /// Refreshes all UI properties to reflect the new configuration.
    /// </summary>
    private void OnConfigUpdated()
    {
        // run on UI thread and reload config
        Dispatcher.Invoke(() => LoadConfigFromBL());
    }

    /// <summary>
    /// Loads configuration from the Business Logic layer and updates all UI properties.
    /// Called on window initialization to populate the UI with current BL state.
    /// </summary>
    /// needs checking
    private void LoadConfigFromBL()
    {
        try
        {
            var config = s_bl.Admin.GetConfig();
            
            // Update all properties - this should trigger UI updates through data bindings
            CurrentTime = s_bl.Admin.GetClock(); // Set initial time from BL
            Configuration = config; // Update the Configuration object
            AdminId = config.AdminId;
            CompanyName = config.CompanyName;
            MaxDist = config.MaxDist ?? 0.0;
            AvgCarMPH = config.AvgCarMPH;
            AvgMotorcycleMPH = config.AvgMotorcycleMPH;
            AvgBikeMPH = config.AvgBicycleMPH;
            AvgWalkMPH = config.AvgWalkMPH;
            MaxDeliveryTime = config.MaxDelTime;
            RiskRange = config.RiskRange;
            DownTime = config.DownTime;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading configuration: {ex.Message}\n\nMake sure to click 'Initialize' button first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }



    #region buttons for clock manipulation
    private void btnAddOneSec_Click(object sender, RoutedEventArgs e)
    {
        s_bl.Admin.ForwardClock(BO.Time.Second);
        CurrentTime = s_bl.Admin.GetClock(); // Refresh the time display
    }

    private void btnAddOneMin_Click(object sender, RoutedEventArgs e)
    {
        s_bl.Admin.ForwardClock(BO.Time.Minute);
        CurrentTime = s_bl.Admin.GetClock(); // Refresh the time display
    }

    private void btnAddOneHr_Click(object sender, RoutedEventArgs e)
    {
        s_bl.Admin.ForwardClock(BO.Time.Hour);
        CurrentTime = s_bl.Admin.GetClock(); // Refresh the time display
    }

    private void btnAddOneDay_Click(object sender, RoutedEventArgs e)
    {
        s_bl.Admin.ForwardClock(BO.Time.Day);
        CurrentTime = s_bl.Admin.GetClock(); // Refresh the time display
    }

    private void btnAddOneYr_Click(object sender, RoutedEventArgs e)
    {
        s_bl.Admin.ForwardClock(BO.Time.Year);
        CurrentTime = s_bl.Admin.GetClock(); // Refresh the time display
    }
    #endregion


    #region database buttons handlers

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

                // Refresh the UI after initialization
                LoadConfigFromBL();

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

                // Refresh the UI after reset
                LoadConfigFromBL();

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

    private void btnListDisplay_Click(object sender, RoutedEventArgs e)
    {
        new CourierListWindow().Show();
    }

    #endregion
}
