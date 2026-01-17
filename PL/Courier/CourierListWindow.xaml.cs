using BO;
using PL.Helpers;
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
    /// <summary>
    /// Initializes a new instance of the CourierListWindow class.
    /// </summary>
    /// <remarks>This constructor sets up the window and its components for displaying or managing a list of
    /// couriers. Use this constructor when creating a new CourierListWindow in your application.</remarks>
    public CourierListWindow()
    {
        InitializeComponent();
    }

    // Static reference to the business logic layer
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
    private readonly int _userId = s_bl.Admin.GetConfig().AdminId;
    private readonly ObserverMutex _mutex = new(); 


    public BO.CourierInList? SelectedCouriers { get; set; }
    public BO.ShippingMethod FilterShippingMethods { get; set; } = BO.ShippingMethod.None;

    public static readonly DependencyProperty CourierListProperty =
        DependencyProperty.Register("CourierList", typeof(IEnumerable<BO.CourierInList>), typeof(CourierListWindow), new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the collection of couriers available for selection or display.
    /// </summary>
    public IEnumerable<BO.CourierInList> CourierList
    {
        get { return (IEnumerable<BO.CourierInList>)GetValue(CourierListProperty); }
        set { SetValue(CourierListProperty, value); }
    }

    

   /// <summary>
   /// Handles the SelectionChanged event of the ComboBox control and updates the courier list accordingly.
   /// </summary>
   /// <param name="sender">The source of the event, typically the ComboBox whose selection has changed.</param>
   /// <param name="e">The event data that contains information about the selection change.</param>
    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => courierListObserver();

    /// <summary>
    /// Private helper method to query and filter the courier list
    /// </summary>
    private void queryCourierList()
    {
        try
        {
            var allCouriers = s_bl?.Courier.GetListOfCouriers(_userId, null, null)!;

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
    /// Observer method to update the courier list when notified
    /// </summary>
    private void courierListObserver()
    {
        if (_mutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;
        _ = Dispatcher.BeginInvoke(async () =>
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
            if (await _mutex.UnsetLoadInProgressAndCheckRestartRequested())
                courierListObserver();
        });
    }

    /// <summary>
    /// Handles the window loaded event to initialize the courier list and subscribe to updates.
    /// </summary>
    /// <param name="sender">name of the sender</param>
    /// <param name="e">event arguments</param>
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            s_bl?.Courier.AddObserver(_userId, courierListObserver);
            queryCourierList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error initializing courier list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Handles the window closed event to unsubscribe from courier list updates.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
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

    /// <summary>
    /// Handles the MouseDoubleClick event for the couriers list and opens the details window for the selected courier.
    /// </summary>
    /// <remarks>If no courier is selected when the event occurs, no action is taken. An error message is
    /// displayed if the courier details window cannot be opened.</remarks>
    /// <param name="sender">The source of the event, typically the couriers list control.</param>
    /// <param name="e">The event data associated with the mouse double-click action.</param>
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

    /// <summary>
    /// Handles the Click event of the Add button by opening a new CourierWindow.
    /// </summary>
    /// <param name="sender">The source of the event, typically the Add button.</param>
    /// <param name="e">The event data associated with the Click event.</param>
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
            s_bl.Courier.DeleteCourier(_userId, courierId);
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
