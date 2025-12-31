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
        try
        {
            var allCouriers = s_bl?.Courier.GetListOfCouriers(123456789, null, null)!;

            // Filter by ShippingMethod in UI
            CourierList = (FilterShippingMethods == BO.ShippingMethod.None) ?
                allCouriers :
                allCouriers.Where(c => c.ShippingMethod == FilterShippingMethods);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading courier list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            CourierList = null;
        }
    }

    /// <summary>
    /// Private observer method - called by BL when the courier list is updated
    /// </summary>
    private void courierListObserver()
    {
        try
        {
            queryCourierList();
        }
        catch (Exception ex)
        {
            Dispatcher.Invoke(() =>
                MessageBox.Show($"Error updating courier list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error));
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            s_bl?.Courier.AddObserver(courierListObserver);
            queryCourierList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error initializing courier list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        try
        {
            s_bl?.Courier.RemoveObserver(courierListObserver);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error while unsubscribing from updates: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public BO.CourierInList? SelectedCouriers { get; set; }

    private void lsvCouriersList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (SelectedCouriers != null)
                new CourierWindow(SelectedCouriers.CourierId).Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening courier details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnAdd_Click(object sender, RoutedEventArgs e)
    {
        new CourierWindow().Show();
    }

    /// <summary>
    /// Handles the delete button click event for removing a courier from the list.
    /// </summary>
    private void btnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not int courierId)
            return;

        // Confirm deletion with the user
        MessageBoxResult result = MessageBox.Show(
            $"Are you sure you want to delete courier ID {courierId}?",
            "Confirm Deletion",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            // Attempt to delete the courier
            s_bl.Courier.DeleteCourier(123456789, courierId);
            MessageBox.Show("Courier deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error deleting courier: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

    }
}
