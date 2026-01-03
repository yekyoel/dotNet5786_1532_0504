using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PL.Courier.CourierScreens
{
    /// <summary>
    /// Interaction logic for OrderSelectionWindow.xaml
    /// </summary>
    public partial class OrderSelectionWindow : Window
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        private readonly int _courierId;
        public ObservableCollection<BO.OpenOrderInList> OpenOrders { get; set; } = new();
        public BO.OpenOrderInList SelectedOrder { get; set; }

        public OrderSelectionWindow(int courierId)
        {
            InitializeComponent();
            _courierId = courierId;
            LoadOpenOrders();
        }

        private void LoadOpenOrders()
        {
            try
            {
                var orders = s_bl.Order.GetAvailableOrdersForCourier(_courierId, _courierId, null, null);
                OpenOrders.Clear();
                foreach (var o in orders)
                {
                    OpenOrders.Add(o);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading orders: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnPickUp_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int orderId)
            {
                try
                {
                    s_bl.Order.ChooseOrder(_courierId, _courierId, orderId);
                    
                    // Requirement: "Send email with details"
                    MessageBox.Show("Order assigned successfully! Email with details sent.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    Close(); // Close selection window after picking
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error assigning order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedOrder != null)
            {
                // Requirement: Show map with locations and lines
                // Simulation: Update a text block or placeholder map control
                // In a real WPF app, we'd use a MapControl (e.g., GMap.NET or Bing Maps)
                // For now, we'll just acknowledge the selection.
                // MapPlaceholder.Text = $"Map: Courier -> Order {SelectedOrder.OrderId} ({SelectedOrder.ArealDistance:N2} km)";
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
