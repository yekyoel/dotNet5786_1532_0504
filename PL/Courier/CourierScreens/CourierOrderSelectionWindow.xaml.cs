using PL.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PL.Courier.CourierScreens;

public partial class CourierOrderSelectionWindow : Window
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
    private readonly ObserverMutex _mutex = new(); //stage 7


    private readonly int _courierId;
    private bool _isObserverRegistered;
    private readonly Action _orderListObserver;

    public ObservableCollection<BO.OpenOrderInList> OpenOrders { get; } = new();

    public BO.OpenOrderInList? SelectedOrder { get; set; }

    public CourierOrderSelectionWindow(int courierId)
    {
        InitializeComponent();
        _courierId = courierId;

        _orderListObserver = RefreshOrdersFromObserver;
        _ = LoadOpenOrdersAsync();
        DataContext = this;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            s_bl.Order.AddObserver(_orderListObserver);
            _isObserverRegistered = true;
        }
        catch
        {
            _isObserverRegistered = false;
        }
    }

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

    // add mutex etc...
    private void RefreshOrdersFromObserver()
    {
        try
        {
            Dispatcher.Invoke(async () => await LoadOpenOrdersAsync());
        }
        catch
        {
            // ignore observer exceptions
        }
    }

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

            DataContext = null;
            DataContext = this;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading available orders: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

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

    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
