using PL.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PL.Courier.CourierScreens;

public partial class CourierOrderSelectionWindow : Window
{
    // BL Static Reference
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
    private readonly ObserverMutex _mutex = new(); 

    // PL properties
    private readonly int _courierId;
    private bool _isObserverRegistered;
    private readonly Action _orderListObserver;

    /// <summary>
    /// Gets the collection of open orders associated with the current context.
    /// </summary>
    /// <remarks>The returned collection is observable, allowing clients to monitor changes such as additions
    /// or removals of open orders. The collection is read-only; to modify its contents, use the appropriate methods
    /// provided by the containing class.</remarks>
    public ObservableCollection<BO.OpenOrderInList> OpenOrders { get; } = new();

    /// <summary>
    /// Initializes a new instance of the CourierOrderSelectionWindow class for the specified courier.
    /// </summary>
    /// <remarks>This constructor sets up the window to display and manage open orders assigned to the
    /// specified courier.</remarks>
    /// <param name="courierId">The unique identifier of the courier for whom open orders will be displayed.</param>
    public CourierOrderSelectionWindow(int courierId)
    {
        InitializeComponent();
        _courierId = courierId;

        _orderListObserver = CourierOrderSelectionObserver;
    }

    /// <summary>
    /// Handles the Loaded event of the window to register the order list observer.
    /// </summary>
    /// <remarks>This method is typically used to perform initialization tasks that require the window to be
    /// fully loaded. If observer registration fails, the observer will not receive updates until registration is
    /// retried.</remarks>
    /// <param name="sender">The source of the event, typically the window being loaded.</param>
    /// <param name="e">The event data associated with the Loaded event.</param>
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            s_bl.Order.AddObserver(_orderListObserver);
            _isObserverRegistered = true;
            _orderListObserver(); // Trigger initial load safely
        }
        catch
        {
            _isObserverRegistered = false;
        }
    }

    /// <summary>
    /// Handles the Closed event of the window to perform necessary cleanup operations.
    /// </summary>
    /// <remarks>This method removes the registered observer from the order list when the window is closed. It
    /// is intended to prevent resource leaks by ensuring that event subscriptions are properly cleaned up.</remarks>
    /// <param name="sender">The source of the event, typically the window being closed.</param>
    /// <param name="e">An EventArgs object that contains the event data.</param>
    private void Window_Closed(object sender, EventArgs e)
    {
        if (!_isObserverRegistered)
            return;

        try
        {
            s_bl.Order.RemoveObserver(_orderListObserver);
        }
        catch
        {
            // ignore errors on close
        }
    }

    /// <summary>
    /// Observes and manages the process of loading available courier orders, handling concurrent load requests and restart
    /// conditions.
    /// </summary>
    /// <remarks>This method coordinates the loading of open orders by ensuring that only one load operation is in
    /// progress at a time. If a restart is requested during a load, the method will automatically restart the loading
    /// process after the current operation completes. Any errors encountered during the loading process are displayed to
    /// the user in a message box. This method is intended for internal use and is not thread-safe for direct external
    /// invocation.</remarks>
    private void CourierOrderSelectionObserver()
    {
        if (_mutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;
        _ = Dispatcher.Invoke(async () =>
        {
            try
            {
                await LoadOpenOrdersAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading available orders: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (await _mutex.UnsetLoadInProgressAndCheckRestartRequested())
                CourierOrderSelectionObserver();
        });
    }

    /// <summary>
    /// Asynchronously loads the list of available orders for the current courier and updates the OpenOrders collection.
    /// </summary>
    /// <remarks>If no available orders are found, a message box is displayed to inform the user of possible
    /// reasons, such as no orders in the database, restrictive distance filters, or all orders already being assigned.
    /// If an error occurs during loading, an error message is shown to the user.</remarks>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    private async Task LoadOpenOrdersAsync()
    {
        try
        {
            var orders = await s_bl.Order.GetAvailableOrdersForCourierAsync(_courierId, _courierId, null, null);

            OpenOrders.Clear();
            foreach (var o in orders)
                OpenOrders.Add(o);

            if (OpenOrders.Count == 0)
            {
                MessageBox.Show(
                    "No available orders found.\n\nCommon reasons:\n" +
                    "- There are no orders in the DB (click Initialize)\n" +
                    "- Your MaxDist is too small (distance filter)\n" +
                    "- All orders already have deliveries that are not Pending/unassigned",
                    "No Orders",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading available orders: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Handles the Click event of the Collect button, assigning the selected order to the courier and notifying the
    /// user of the result.
    /// </summary>
    /// <remarks>If the order assignment is successful, a confirmation message is displayed and the window is
    /// closed. If an error occurs, an error message is shown and the list of open orders is refreshed. This handler
    /// expects the sender to be a Button with a valid integer order ID in its Tag property; otherwise, the method
    /// returns without performing any action.</remarks>
    /// <param name="sender">The source of the event, expected to be a Button with its Tag property set to the order ID as an integer.</param>
    /// <param name="e">The event data associated with the Click event.</param>
    private async void btnCollect_Click(object sender, RoutedEventArgs e)    
    {
        if (sender is not Button btn || btn.Tag is not int orderId)
            return;

        try
        {
             await s_bl.Order.ChooseOrderAsync(_courierId, _courierId, orderId);

            MessageBox.Show("Order assigned successfully! Email with details sent.",
                "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error assigning order: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            _ = LoadOpenOrdersAsync();
        }
    }

    /// <summary>
    /// Handles the Click event of the Close button and closes the window.
    /// </summary>
    /// <param name="sender">The source of the event, typically the Close button.</param>
    /// <param name="e">The event data associated with the Click event.</param>
    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
