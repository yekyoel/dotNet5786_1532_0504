using System;
using System.Windows;

namespace PL.Order
{
    /// <summary>
    /// Interaction logic for AddOrderWindow.xaml
    /// </summary>
    public partial class AddOrderWindow : Window
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        public BO.Order NewOrder
        {
            get { return (BO.Order)GetValue(NewOrderProperty); }
            set { SetValue(NewOrderProperty, value); }
        }

        public static readonly DependencyProperty NewOrderProperty =
            DependencyProperty.Register("NewOrder", typeof(BO.Order), typeof(AddOrderWindow), new PropertyMetadata(null));

        public AddOrderWindow()
        {
            InitializeComponent();
            NewOrder = new BO.Order
            {
                OrderPlacedTime = DateTime.Now
            };
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ValidateOrderOrThrow(NewOrder);

                // admin user id for now
                int adminId = s_bl.Admin.GetConfig().AdminId;
                s_bl.Order.AddOrder(adminId, NewOrder);

                MessageBox.Show("Order added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void ValidateOrderOrThrow(BO.Order order)
        {
            if (order is null)
                throw new ArgumentNullException(nameof(order), "Order cannot be null.");

            if (string.IsNullOrWhiteSpace(order.CustomerName))
                throw new ArgumentException("Customer Name is required.", nameof(order.CustomerName));

            if (string.IsNullOrWhiteSpace(order.CustomerPhone))
                throw new ArgumentException("Customer Phone is required.", nameof(order.CustomerPhone));

            if (string.IsNullOrWhiteSpace(order.OrderAddress))
                throw new ArgumentException("Order Address is required.", nameof(order.OrderAddress));

            if (order.Weight <= 0)
                throw new ArgumentException("Weight must be a positive number.", nameof(order.Weight));

            if (order.Latitude < -90.0 || order.Latitude > 90.0)
                throw new ArgumentException("Latitude must be between -90 and 90.", nameof(order.Latitude));

            if (order.Longitude < -180.0 || order.Longitude > 180.0)
                throw new ArgumentException("Longitude must be between -180 and 180.", nameof(order.Longitude));

            if (order.OrderTyype is null)
                throw new ArgumentException("Order Type is required.", nameof(order.OrderTyype));
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
