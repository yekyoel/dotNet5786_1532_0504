using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PL.Delivery
{
    /// <summary>
    /// Interaction logic for DeliveryListWindow.xaml
    /// </summary>
    public partial class DeliveryListWindow : Window
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        private readonly int _userId; 

        public ObservableCollection<BO.OrderInList> DeliveryList { get; set; } = new();
        public BO.OrderStatus? FilterStatus { get; set; }
        public BO.OrderInList? SelectedDelivery { get; set; }

        public DeliveryListWindow()
        {
            InitializeComponent();
            _userId = s_bl.Admin.GetConfig().AdminId; // Default to admin for this context
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDeliveries();
        }

        private void LoadDeliveries()
        {
            try
            {
                // Get all orders (deliveries)
                var deliveries = s_bl.Order.GetListOfOrders(_userId, null, null, null);
                
                // Apply filter if selected
                if (FilterStatus.HasValue)
                {
                    deliveries = deliveries.Where(d => d.OrderStatus == FilterStatus.Value);
                }

                DeliveryList.Clear();
                foreach (var d in deliveries)
                {
                    DeliveryList.Add(d);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading deliveries: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadDeliveries();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            new DeliveryWindow().ShowDialog();
            LoadDeliveries();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int orderId)
            {
                if (MessageBox.Show("Are you sure you want to cancel this delivery?", "Confirm Cancellation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        s_bl.Order.CancelOrder(_userId, orderId);
                        LoadDeliveries();
                        MessageBox.Show("Delivery cancelled successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error cancelling delivery: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedDelivery != null)
            {
                new DeliveryWindow(SelectedDelivery.OrderId).ShowDialog();
                LoadDeliveries();
            }
        }
        
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Optional: Handle selection logic if needed
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // Cleanup if needed
        }
    }
}
