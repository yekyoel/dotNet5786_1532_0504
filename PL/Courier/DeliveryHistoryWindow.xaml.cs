using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace PL.Courier.CourierScreens
{
    /// <summary>
    /// Interaction logic for DeliveryHistoryWindow.xaml
    /// </summary>
    public partial class DeliveryHistoryWindow : Window
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        private readonly int _courierId;
        public ObservableCollection<BO.ClosedDeliveryInList> HistoryList { get; set; } = new();

        public DeliveryHistoryWindow(int courierId)
        {
            InitializeComponent();
            _courierId = courierId;
            LoadHistory();
        }

        private void LoadHistory()
        {
            try
            {
                var history = s_bl.Order.GetCompletedCourierDeliveries(_courierId, _courierId, null, null);
                HistoryList.Clear();
                foreach (var h in history)
                {
                    HistoryList.Add(h);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading history: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
