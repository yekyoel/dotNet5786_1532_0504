using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PL.Courier.CourierScreens;

public partial class CourierOrderSelectionWindow : Window
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    private readonly int _courierId;
    private bool _isObserverRegistered;
    private readonly Action _orderListObserver;

    public ObservableCollection<BO.OpenOrderInList> OpenOrders { get; } = new();

    public BO.OpenOrderInList? SelectedOrder { get; set; }

    public string MapStatusText { get; set; } = "Select an order to preview the route.";
    public string MapCoordinatesText { get; set; } = string.Empty;

    public CourierOrderSelectionWindow(int courierId)
    {
        InitializeComponent();
        _courierId = courierId;

        // Suppress script error popups from the legacy WebBrowser control
        try
        {
            dynamic activeX = MapBrowser.GetType().InvokeMember(
                "ActiveXInstance",
                System.Reflection.BindingFlags.GetProperty |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic,
                null,
                MapBrowser,
                new object[] { });

            if (activeX is not null)
                activeX.Silent = true;
        }
        catch
        {
            // best-effort; ignore failures here
        }

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

    private void btnCollect_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not int orderId)
            return;

        try
        {
            s_bl.Order.ChooseOrder(_courierId, _courierId, orderId);

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

    private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateMapPreview();
    }

    private void UpdateMapPreview()
    {
        if (SelectedOrder is null)
        {
            MapStatusText = "Select an order to preview the route.";
            MapCoordinatesText = string.Empty;

            // clear map
            MapBrowser.Navigate("about:blank");

            DataContext = null;
            DataContext = this;
            return;
        }

        try
        {
            var cfg = s_bl.Admin.GetConfig();

            var order = s_bl.Order.GetOrderDetails(_courierId, SelectedOrder.OrderId);

            MapStatusText =
                $"Company → Order #{SelectedOrder.OrderId} ({SelectedOrder.TypeOrder})\n" +
                $"Delivery method: (based on courier shipping method)";

            MapCoordinatesText =
                $"Company: lat={cfg.Latitude:0.00000}, lon={cfg.Longitude:0.00000}\n" +
                $"Order: lat={order.Latitude:0.00000}, lon={order.Longitude:0.00000}\n" +
                $"Air distance: {SelectedOrder.ArealDistance:0.00} km\n" +
                $"Route distance: {SelectedOrder.ActualDistance:0.00} km";

            // Simple OpenStreetMap directions URL
            if (cfg.Latitude.HasValue && cfg.Longitude.HasValue)
            {
                string url =
                    $"https://www.openstreetmap.org/directions?engine=fossgis_osrm_car&route=" +
                    $"{cfg.Latitude.Value:0.00000}%2C{cfg.Longitude.Value:0.00000}%3B" +
                    $"{order.Latitude:0.00000}%2C{order.Longitude:0.00000}";

                MapBrowser.Navigate(url);
            }

            DataContext = null;
            DataContext = this;
        }
        catch (Exception ex)
        {
            MapStatusText = $"Map preview error: {ex.Message}";
            MapCoordinatesText = string.Empty;
            MapBrowser.Navigate("about:blank");
            DataContext = null;
            DataContext = this;
        }
    }

    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
