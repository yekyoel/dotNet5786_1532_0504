using PL.Courier;
using PL.Helpers;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace PL.Order;

/// <summary>
/// Interaction logic for OrderWindow.xaml
/// </summary>
public partial class OrderWindow : Window
{
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
    private readonly ObserverMutex _mutex = new(); //stage 7
    private int adminId= s_bl.Admin.GetConfig().AdminId;
   
    bool _isObserverRegistered;

    /// <summary>
    /// Gets or sets the currently selected order.
    /// </summary>
    public BO.Order? CurrentOrder
    {
        get { return (BO.Order?)GetValue(CurrentOrderProperty); }
        set { SetValue(CurrentOrderProperty, value); }
    }

    public static readonly DependencyProperty CurrentOrderProperty =
        DependencyProperty.Register("CurrentOrder", typeof(BO.Order), typeof(OrderWindow), new PropertyMetadata(null));

    
    public static readonly DependencyProperty ButtonTextProperty =
        DependencyProperty.Register("ButtonText", typeof(string), typeof(OrderWindow));

    /// <summary>
    /// Gets or sets the text displayed on the action button (Add/Update).
    /// </summary>
    public string ButtonText
    {
        get { return (string)GetValue(ButtonTextProperty); }
        set { SetValue(ButtonTextProperty, value); }
    }

    public bool IsUpdateMode
    {
        get { return (bool)GetValue(IsUpdateModeProperty); }
        set { SetValue(IsUpdateModeProperty, value); }
    }

    public static readonly DependencyProperty IsUpdateModeProperty =
        DependencyProperty.Register("IsUpdateMode", typeof(bool), typeof(OrderWindow), new PropertyMetadata(false));


    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }

    public static readonly DependencyProperty IsBusyProperty =
        DependencyProperty.Register(
            "IsBusy",
            typeof(bool),
            typeof(OrderWindow),
            new PropertyMetadata(false, OnIsBusyChanged));

    private static void OnIsBusyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not OrderWindow window)
            return;

        bool isBusy = (bool)e.NewValue;
        if (window.BusyOverlay != null)
        {
            window.BusyOverlay.Visibility = isBusy ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Initializes a new instance of the OrderWindow class in update mode.
    /// </summary>
    /// <param name="id">The existing order ID to edit. Value must be greater than zero.</param>
    public OrderWindow(int id = 0)
    {
        ButtonText = id == 0 ? "Add" : "Update";
        IsUpdateMode = id != 0;
        InitializeComponent();

        if (id != 0)
        {
            try
            {
                CurrentOrder = s_bl.Order.GetOrderDetails(adminId, id)!;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }
    }

    /// <summary>
    /// Handles the Loaded event of the window to perform initialization tasks when the window is first displayed.
    /// </summary>
    /// <param name="sender">The source of the event, typically the window being loaded.</param>
    /// <param name="e">The event data associated with the Loaded event.</param>
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var order = CurrentOrder;
            if (order?.Id > 0)
            {
                s_bl.Order.AddObserver(order.Id, OrderObserver);
                _isObserverRegistered = true;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error while subscribing to updates: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Handles the Closed event of the window and performs necessary cleanup operations.
    /// </summary>
    /// <remarks>This method ensures that any observers associated with the current order are properly removed when the window is closed.</remarks>
    /// <param name="sender">The source of the event, typically the window being closed.</param>
    /// <param name="e">An EventArgs object that contains the event data.</param>
    private void Window_Closed(object sender, EventArgs e)
    {
        if (!_isObserverRegistered)
            return;

        try
        {
            var order = CurrentOrder;
            if (order?.Id > 0)
                s_bl.Order.RemoveObserver(order.Id, OrderObserver);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error while unsubscribing from updates: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Refreshes the details of the current order by retrieving the latest information from the data source.
    /// </summary>
    /// <remarks>This method updates the CurrentOrder property with the most recent data for the currently selected order.</remarks>
    private void OrderObserver()
    {
        if (_mutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;
        _ = Dispatcher.BeginInvoke(async () =>
        {
            try
            {
                var order = CurrentOrder;
                if (order is null || order.Id <= 0)
                    return;

                CurrentOrder = s_bl.Order.GetOrderDetails(adminId, order.Id);
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                    MessageBox.Show($"Error refreshing order details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error));
            }

            if (await _mutex.UnsetLoadInProgressAndCheckRestartRequested())
                OrderObserver();
        });
    }

    private async void btnAddUpdate_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            IsBusy = true;

            // Validate order before sending to BL
            ValidateOrderOrThrow(CurrentOrder!);

            if (ButtonText == "Add")
            {
                await s_bl.Order.AddOrder(adminId, CurrentOrder!);
                MessageBox.Show("Order added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            else
            {
                s_bl.Order.UpdateOrderDetails(adminId, CurrentOrder!);
                MessageBox.Show("Order updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Validates the order data before adding or updating.
    /// </summary>
    /// <param name="order">The order to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when order is null.</exception>
    /// <exception cref="ArgumentException">Thrown when order properties are invalid.</exception>
    private static void ValidateOrderOrThrow(BO.Order order)
    {
        // id is given in dal 

        // Description
        if (string.IsNullOrWhiteSpace(order.Description))
            throw new ArgumentException("Description is required.", nameof(order.Description));

        // Order Address
        if (string.IsNullOrWhiteSpace(order.OrderAddress))
            throw new ArgumentException("Order Address is required.", nameof(order.OrderAddress));

        // Arial Distance
        //if (order.AerialDistance < 0)
        //   throw new ArgumentException("Arial Distance cannot be negative.", nameof(order.AerialDistance));

        // Customer Name
        if (string.IsNullOrWhiteSpace(order.CustomerName))
            throw new ArgumentException("Customer Name is required.", nameof(order.CustomerName));

        // Customer Phone
        if (string.IsNullOrWhiteSpace(order.CustomerPhone))
            throw new ArgumentException("Customer Phone is required.", nameof(order.CustomerPhone));

        // Weight
        if (order.Weight <= 0)
            throw new ArgumentException("Weight must be a positive number.", nameof(order.Weight));

        // orderTime 
        // maxDeliveryTime
        // timeleft 


    }

    /// <summary>
    /// Closes the order window.
    /// </summary>
    private void btnClose_Click(object sender, RoutedEventArgs e) => Close();

}
