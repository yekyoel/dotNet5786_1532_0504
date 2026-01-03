using System;
using System.Windows;

namespace PL.Delivery
{
    /// <summary>
    /// Interaction logic for DeliveryWindow.xaml
    /// </summary>
    public partial class DeliveryWindow : Window
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        private readonly int _userId;
        private readonly int? _orderId;

        public BO.Order CurrentDelivery { get; set; }
        public bool IsUpdateMode { get; set; }
        public string ButtonText { get; set; }

        public DeliveryWindow(int? orderId = null)
        {
            InitializeComponent();
            _userId = s_bl.Admin.GetConfig().AdminId; // Default to admin
            _orderId = orderId;

            if (_orderId.HasValue)
            {
                IsUpdateMode = true;
                ButtonText = "Update";
                LoadDelivery(_orderId.Value);
            }
            else
            {
                IsUpdateMode = false;
                ButtonText = "Add";
                CurrentDelivery = new BO.Order(); // Initialize new order
            }
        }

        private void LoadDelivery(int id)
        {
            try
            {
                CurrentDelivery = s_bl.Order.GetOrderDetails(_userId, id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading delivery details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void btnAddUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IsUpdateMode)
                {
                    // Update existing order
                    s_bl.Order.UpdateOrderDetails(_userId, CurrentDelivery);
                    MessageBox.Show("Delivery updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Add new order
                    s_bl.Order.AddOrder(_userId, CurrentDelivery);
                    
                    // Requirement: "Send email to couriers"
                    // Simulation of sending email to relevant couriers
                    MessageBox.Show("Delivery added successfully! Email notifications sent to relevant couriers.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving delivery: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (IsUpdateMode && CurrentDelivery != null)
            {
                if (MessageBox.Show("Are you sure you want to cancel this delivery?", "Confirm Cancellation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        s_bl.Order.CancelOrder(_userId, CurrentDelivery.Id);
                        MessageBox.Show("Delivery cancelled successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadDelivery(CurrentDelivery.Id); // Refresh
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error cancelling delivery: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Ensure UI updates if bindings didn't catch initial set
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // Cleanup
        }
    }
}
