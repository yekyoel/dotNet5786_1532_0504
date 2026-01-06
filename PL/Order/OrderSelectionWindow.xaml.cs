using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace PL.Order
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
                    MessageBox.Show("Order assigned successfully! Email with details sent.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error assigning order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Placeholder for map update logic if needed
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
