using BO;
using PL.Helpers;
using PL.Order;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PL.Order;

/// <summary>
/// Represents a window that displays and manages a list of orders, providing functionality to filter, view, add, and
/// cancel orders within the application's user interface.
/// </summary>
/// <remarks>OrderListWindow enables users to interact with order data through various controls, including
/// filtering by status, viewing order details, and performing actions such as adding or cancelling orders. The window
/// subscribes to updates from the business logic layer to keep the displayed order list current and ensures proper
/// resource management by unsubscribing from notifications when closed. Error handling is provided throughout to inform
/// users of any issues encountered during operations.</remarks>
public partial class OrderListWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the OrderListWindow class.
    /// </summary>
    public OrderListWindow()
    {
        InitializeComponent();
    }

    // BL/access variables and properties
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
    private readonly int _userId = s_bl.Admin.GetConfig().AdminId;
    private readonly ObserverMutex _mutex = new();

    #region PL properties 

    public BO.OrderInList? SelectedOrders { get; set; }
    public BO.OrderStatus FilterStatus { get; set; } = BO.OrderStatus.None;
    public BO.ScheduleStatus? FilterScheduleStatus { get; set; }
    private bool _isGrouped; // Toggle from UI: when true, group by OrderType; when false, no grouping

    /// <summary>
    /// Gets or sets a value indicating whether items are displayed in groups.
    /// </summary>
    public bool IsGrouped
    {
        get => _isGrouped;
        set
        {
            if (_isGrouped == value)
                return;
            _isGrouped = value;
            ApplyGrouping();
        }
    }

    /// <summary>
    /// Gets or sets the collection of orders to be displayed in the list.
    /// </summary>
    /// <remarks>The collection represents the current set of orders shown in the user interface.
    /// Assigning a new collection updates the displayed orders. The property should be set to a non-null
    /// enumerable; assigning null may result in no orders being shown.</remarks>
    public IEnumerable<BO.OrderInList> OrderInList
    {
        get { return (IEnumerable<BO.OrderInList>)GetValue(OrderListProperty); }
        set { SetValue(OrderListProperty, value); }
    }


    // DependencyProperty for OrderInList
    public static readonly DependencyProperty OrderListProperty =
        DependencyProperty.Register("OrderInList", typeof(IEnumerable<BO.OrderInList>), typeof(OrderListWindow), new PropertyMetadata(null));

    #endregion


    /// <summary>
    /// Handles the SelectionChanged event of the ComboBox control and updates the order list based on the new
    /// selection.
    /// </summary>
    /// <param name="sender">The source of the event, typically the ComboBox whose selection has changed.</param>
    /// <param name="e">An object that contains event data for the selection change, including information about the items that were
    /// added or removed.</param>
    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => OrderListObserver();

    /// <summary>
    /// Loads the list of orders for the current user and applies the selected status filter, if any.
    /// </summary>
    /// <remarks>This method retrieves all orders associated with the current user and updates the
    /// order list to reflect the applied status filter. If an error occurs during loading, an error message is
    /// displayed to the user. This method is intended for internal use within the class and does not return a
    /// value.</remarks>
    private void LoadOrderList()
    {
        try
        {
            // Get all orders (deliveries)
            var Orders = s_bl.Order.GetListOfOrders(_userId, null, null, null)!; // check userid probably sync with login 

            // Apply filters
            IEnumerable<BO.OrderInList> filtered = Orders;

            if (FilterStatus != BO.OrderStatus.None)
                filtered = filtered.Where(c => c.OrderStatus == FilterStatus);

            if (FilterScheduleStatus != null)
                filtered = filtered.Where(c => c.ScheduleStatus == FilterScheduleStatus);
            
            OrderInList = filtered;
            
            ApplyGrouping();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading deliveries: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    /// <summary>
    /// Configures the collection view to group items by order type if grouping is enabled.
    /// </summary>
    /// <remarks>This method clears any existing group descriptions on the collection view associated with the
    /// OrderInList collection. If the IsGrouped property is set to <see langword="true"/>, the view will group items
    /// based on the OrderType property. This affects how items are displayed in UI controls that use the collection
    /// view, such as data grids or list views.</remarks>
    private void ApplyGrouping()
    {
        var view = CollectionViewSource.GetDefaultView(OrderInList);
        if (view == null)
            return;

        view.GroupDescriptions.Clear();

        if (IsGrouped)
        {
            view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(BO.OrderInList.OrderType)));
        }
    }

    /// <summary>
    /// Attempts to refresh the order list and displays an error message if the update fails.
    /// </summary>
    /// <remarks>This method handles exceptions that occur during the order list update by showing an
    /// error dialog to the user. It is intended to be used in contexts where user feedback is required upon
    /// failure.</remarks>
    private void OrderListObserver()
    {
        if (_mutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;
        _ = Dispatcher.BeginInvoke(async () =>
        { 
            try
            {
                LoadOrderList();
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                    MessageBox.Show($"Error updating Order list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error));
            }
            if(await _mutex.UnsetLoadInProgressAndCheckRestartRequested())
                OrderListObserver();
        });
    }

    /// <summary>
    /// Handles the Loaded event of the window to initialize the order observer and load the order list.
    /// </summary>
    /// <remarks>This method sets up necessary observers and loads initial data when the window is
    /// displayed. If initialization fails, an error message is shown to the user.</remarks>
    /// <param name="sender">The source of the event, typically the window that has finished loading.</param>
    /// <param name="e">The event data associated with the Loaded event.</param>
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            s_bl?.Order.AddObserver(OrderListObserver);
            LoadOrderList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error initializing Order list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Handles the window's Closed event by unsubscribing from order list updates.
    /// </summary>
    /// <remarks>This method ensures that the observer for order list updates is removed when the
    /// window is closed, preventing further notifications. If an error occurs during unsubscription, an error
    /// message is displayed to the user.</remarks>
    /// <param name="sender">The source of the event, typically the window that was closed.</param>
    /// <param name="e">An <see cref="EventArgs"/> instance containing event data.</param>
    private void Window_Closed(object sender, EventArgs e)
    {
        try
        {
            s_bl?.Order?.RemoveObserver(OrderListObserver);
        }
        catch (Exception ex)
        {
            // Log exception but don't crash on window close
            System.Diagnostics.Debug.WriteLine($"Error while unsubscribing from order updates: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles the Click event of the Add button by opening a new order window.
    /// </summary>
    /// <param name="sender">The source of the event, typically the Add button control.</param>
    /// <param name="e">The event data associated with the button click.</param>
    private void btnAdd_Click(object sender, RoutedEventArgs e)
    {
        new OrderWindow().Show();
    }

    /// <summary>
    /// Handles the click event of the Cancel button to prompt the user for confirmation and, if confirmed, cancels
    /// the associated delivery order.
    /// </summary>
    /// <remarks>If the user confirms cancellation, the method attempts to cancel the delivery order
    /// and updates the order list. Displays a success or error message based on the outcome. This event handler is
    /// typically wired to a Cancel button in a delivery management interface.</remarks>
    /// <param name="sender">The source of the event, expected to be a Button with its Tag property containing the order ID to cancel.</param>
    /// <param name="e">The event data associated with the button click.</param>
    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is int orderId)
        {
            if (MessageBox.Show("Are you sure you want to cancel this delivery?",
                                "Confirm Cancellation",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    s_bl.Order.CancelOrder(_userId, orderId);
                    LoadOrderList();
                    MessageBox.Show("Delivery cancelled successfully.",
                                    "Success",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error cancelling delivery: {ex.Message}",
                                    "Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
        }
    }

    /// <summary>
    /// Handles the double-click event on the data grid to open the details window for the selected order.
    /// </summary>
    /// <remarks>If no order is selected, no window will be opened. Any errors encountered while
    /// opening the order details window are displayed to the user in a message box.</remarks>
    /// <param name="sender">The source of the event, typically the data grid control that was double-clicked.</param>
    /// <param name="e">The event data associated with the mouse double-click action.</param>
    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (SelectedOrders != null)
                new OrderWindow(SelectedOrders.OrderId).Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening Order details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Optional: Handle selection logic if needed
    }
}

