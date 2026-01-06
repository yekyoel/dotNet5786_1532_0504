using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace PL.Courier.CourierScreens;

public partial class DeliveryHistoryWindow : Window
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    private readonly int _courierId;
    private bool _isObserverRegistered;
    private readonly Action _historyObserver;

    public ObservableCollection<BO.ClosedDeliveryInList> HistoryList { get; } = new();

    public IEnumerable<BO.CompletionType> CompletionTypes { get; } =
        Enum.GetValues<BO.CompletionType>();

    public BO.CompletionType? SelectedCompletionTypeFilter { get; set; }

    public IReadOnlyList<string> SortOptions { get; } = new[]
    {
        "Delivery ID",
        "Order ID",
        "Distance",
        "Duration",
        "Completion Type"
    };

    public string SelectedSortOption { get; set; } = "Delivery ID";

    public DeliveryHistoryWindow(int courierId)
    {
        InitializeComponent();

        _courierId = courierId;
        _historyObserver = HistoryObserver;

        LoadHistory();
        DataContext = this;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            s_bl.Order.AddObserver(_historyObserver);
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
            s_bl.Order.RemoveObserver(_historyObserver);
        }
        catch
        {
            // ignore
        }
    }

    private void HistoryObserver()
    {
        try
        {
            Dispatcher.Invoke(LoadHistory);
        }
        catch
        {
            // ignore
        }
    }

    private void FilterSort_Changed(object sender, EventArgs e)
    {
        LoadHistory();
    }

    private void LoadHistory()
    {
        try
        {
            var history = s_bl.Order.GetCompletedCourierDeliveries(_courierId, _courierId, null, null);

            IEnumerable<BO.ClosedDeliveryInList> query = history;

            // Filter (optional)
            if (SelectedCompletionTypeFilter.HasValue)
                query = query.Where(x => x.CompletionType == SelectedCompletionTypeFilter.Value);

            // Sort
            query = SelectedSortOption switch
            {
                "Order ID" => query.OrderBy(x => x.OrderId),
                "Distance" => query.OrderBy(x => x.ActualDistance),
                "Duration" => query.OrderBy(x => x.TotalCompletionTime),
                "Completion Type" => query.OrderBy(x => x.CompletionType),
                _ => query.OrderBy(x => x.DeliveryId),
            };

            HistoryList.Clear();
            foreach (var item in query)
                HistoryList.Add(item);

            // refresh bindings (DataContext=self approach)
            DataContext = null;
            DataContext = this;
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
