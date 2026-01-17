using PL.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace PL.Courier.CourierScreens;

public partial class DeliveryHistoryWindow : Window
{
    // BL Static Reference properties
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
    private readonly ObserverMutex _mutex = new();

    // PL properties
    private readonly int _courierId;
    private bool _isObserverRegistered;
    private readonly Action _historyObserver;

    /// <summary>
    /// Gets the collection of closed delivery records associated with the current context.
    /// </summary>
    /// <remarks>The returned collection is observable, allowing clients to monitor changes such as additions
    /// or removals of closed delivery items. This property is read-only; to modify the collection, add or remove items
    /// directly from the returned instance.</remarks>
    public ObservableCollection<BO.ClosedDeliveryInList> HistoryList { get; } = new();

    public IEnumerable<BO.CompletionType> CompletionTypes { get; } =
        Enum.GetValues<BO.CompletionType>();

    public BO.CompletionType? SelectedCompletionTypeFilter { get; set; } = BO.CompletionType.None;

    /// <summary>
    /// Gets the list of available sort options for deliveries.
    /// </summary>
    public IReadOnlyList<string> SortOptions { get; } = new[]
    {
        "Delivery ID",
        "Order ID",
        "Distance",
        "Duration",
        "Completion Type"
    };

    public string SelectedSortOption { get; set; } = "Delivery ID";

    public bool IsLoading { get; set; }

    /// <summary>
    /// Initializes a new instance of the DeliveryHistoryWindow class for the specified courier.
    /// </summary>
    /// <param name="courierId">The unique identifier of the courier whose delivery history is to be displayed.</param>
    public DeliveryHistoryWindow(int courierId)
    {
        InitializeComponent();

        _courierId = courierId;
        _historyObserver = HistoryObserver;

        DataContext = this;
        _ = LoadHistoryAsync();
    }

    /// <summary>
    /// Handles the Loaded event of the window to register the history observer with the order system.
    /// </summary>
    /// <remarks>If observer registration fails during window loading, the observer will not receive order
    /// updates until registration is retried.</remarks>
    /// <param name="sender">The source of the event, typically the window being loaded.</param>
    /// <param name="e">The event data associated with the Loaded event.</param>
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

    /// <summary>
    /// Handles the Closed event of the window to perform necessary cleanup operations.
    /// </summary>
    /// <remarks>This method removes an observer from the order history when the window is closed, if it was
    /// previously registered. Exceptions during observer removal are ignored.</remarks>
    /// <param name="sender">The source of the event, typically the window being closed.</param>
    /// <param name="e">An EventArgs object that contains the event data.</param>
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

    /// <summary>
    /// Observes and manages the asynchronous loading of history data, handling concurrent load requests and restart
    /// conditions.
    /// </summary>
    /// <remarks>This method coordinates history loading operations to ensure that only one load is in
    /// progress at a time. If a restart is required during a load, the method will automatically restart the loading
    /// process after the current operation completes. This method is intended for internal use and is not thread-safe
    /// for direct external invocation.</remarks>
    private void HistoryObserver()
    {
        if (_mutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;
        _ = Dispatcher.BeginInvoke(async () =>
        {
            await LoadHistoryAsync();

            if (await _mutex.UnsetLoadInProgressAndCheckRestartRequested())
                HistoryObserver();

        });
    }

    /// <summary>
    /// Handles changes to the filter or sort selection in the user interface.
    /// </summary>
    /// <param name="sender">The source of the event, typically the control whose selection has changed.</param>
    /// <param name="e">The event data containing information about the selection change.</param>
    private void FilterSort_Changed(object sender, SelectionChangedEventArgs e) => HistoryObserver();

    /// <summary>
    /// Asynchronously loads the courier's completed delivery history and updates the history list according to the
    /// selected filters and sort options.
    /// </summary>
    /// <remarks>This method retrieves completed deliveries for the current courier, applies the selected
    /// completion type filter and sort option, and updates the history list accordingly. If an error occurs during
    /// loading, an error message is displayed to the user. The loading state is updated throughout the
    /// operation.</remarks>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    private async Task LoadHistoryAsync()
    {
        try
        {
            IsLoading = true;
            // DataContext = null; DataContext = this;

            var history = await s_bl.Order.GetCompletedCourierDeliveriesAsync(_courierId, _courierId, null, null);

            IEnumerable<BO.ClosedDeliveryInList> query = history;

            // "None" means no filter -> show all
            if (SelectedCompletionTypeFilter is BO.CompletionType filter && filter != BO.CompletionType.None)
                query = query.Where(x => x.CompletionType == filter);

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
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading history: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Handles the Click event of the Close button and closes the window.
    /// </summary>
    /// <param name="sender">The source of the event, typically the Close button.</param>
    /// <param name="e">The event data associated with the Click event.</param>
    private void btnClose_Click(object sender, RoutedEventArgs e) => Close();
 
}
