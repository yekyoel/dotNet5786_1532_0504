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
using PL.Courier;

namespace PL;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    public MainWindow()
    {
        InitializeComponent();

    }

    #region Dependency Properties and CLR Wrappers

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

      

    private void btnAddOneSec_Click(object sender, RoutedEventArgs e)
    {
        s_bl.Admin.ForwardClock(BO.Time.Minute); // TODO: change to second
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
    
}

