using BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PL.Courier;

/// <summary>
/// Interaction logic for CourierListWindow.xaml
/// </summary>
public partial class CourierListWindow : Window
{
    public CourierListWindow()
    {
        InitializeComponent();
    }

    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    public IEnumerable<BO.CourierInList> CourierList
    {
        get { return (IEnumerable<BO.CourierInList>)GetValue(CourierListProperty); }
        set { SetValue(CourierListProperty, value); }
    }

    public static readonly DependencyProperty CourierListProperty =
        DependencyProperty.Register("CourierList", typeof(IEnumerable<BO.CourierInList>), typeof(CourierListWindow), new PropertyMetadata(null));

    public BO.ShippingMethod FilterShippingMethods { get; set; } = BO.ShippingMethod.None;

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        queryCourierList();
    }

    /// <summary>
    /// Private helper method to query and filter the courier list
    /// </summary>
    private void queryCourierList()
    {
        var allCouriers = s_bl?.Courier.GetListOfCouriers(0, null, null)!;

        // Filter by ShippingMethod in UI
        CourierList = (FilterShippingMethods == BO.ShippingMethod.None) ?
            allCouriers :
            allCouriers.Where(c => (BO.ShippingMethod)c.TypeOrder == FilterShippingMethods);
    }

    /// <summary>
    /// Private observer method - called by BL when the courier list is updated
    /// </summary>
    private void courierListObserver()
        => queryCourierList();

    private void Window_Loaded(object sender, RoutedEventArgs e)
        => s_bl?.Courier.AddObserver(courierListObserver);

    private void Window_Closed(object sender, EventArgs e)
        => s_bl?.Courier.RemoveObserver(courierListObserver);
}
