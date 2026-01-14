using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace PL.Courier.CourierScreens;

/// <summary>
/// Interaction logic for CourierMainWindow.xaml
/// </summary>
public partial class CourierMainWindow : Window
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    private readonly int _courierId;
    private bool _isObserverRegistered;

    public BO.Courier? CurrentCourier { get; set; }

    public IEnumerable<BO.CompletionType> CompletionTypes { get; } =
        Enum.GetValues<BO.CompletionType>();

    public BO.CompletionType SelectedCompletionType { get; set; } = BO.CompletionType.None;

    public bool HasOrderInProgress => (CurrentCourier?.OrderInProg?.OrderId ?? 0) != 0;

    public bool HasNoOrderInProgress => !HasOrderInProgress;

    public bool CanSelectOrder => CurrentCourier is not null && CurrentCourier.IsActive && !HasOrderInProgress;

    public bool CanFinishHandling => CurrentCourier is not null
                                     && HasOrderInProgress
                                     && SelectedCompletionType != BO.CompletionType.None;

    public string LastCompletionTypeText { get; set; } = "N/A";

    public CourierMainWindow(int courierId)
    {
        InitializeComponent();
        _courierId = courierId;

        LoadCourierDetails();
        DataContext = this;
    }

    private void CompletionType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        // Force refresh since the window doesn't implement INotifyPropertyChanged
        DataContext = null;
        DataContext = this;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            s_bl.Courier.AddObserver(_courierId, CourierObserver);
            _isObserverRegistered = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error while subscribing to updates: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        if (!_isObserverRegistered)
            return;

        try
        {
            s_bl.Courier.RemoveObserver(_courierId, CourierObserver);
        }
        catch
        {
            // don't crash on close
        }
    }

    private void CourierObserver()
    {
        try
        {
            Dispatcher.Invoke(LoadCourierDetails);
        }
        catch
        {
            // ignore observer exceptions
        }
    }

    private void LoadCourierDetails()
    {
        CurrentCourier = s_bl.Courier.GetCourierDetails(_courierId, _courierId);

        // Reset completion choice when there is no order in progress (so button stays disabled).
        if (!HasOrderInProgress)
            SelectedCompletionType = BO.CompletionType.None;

        LoadLastCompletionType();

        DataContext = null;
        DataContext = this;
    }

    private void btnUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentCourier is null)
            return;

        try
        {
            // Always re-read authoritative record, then copy only allowed fields.
            var authoritative = s_bl.Courier.GetCourierDetails(_courierId, _courierId);

            ApplyCourierEditableFieldsOrThrow(authoritative, CurrentCourier);

            s_bl.Courier.UpdateCourierDetails(_courierId, authoritative);
            MessageBox.Show("Details updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            LoadCourierDetails();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Update Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            LoadCourierDetails(); // revert UI to authoritative state
        }
    }

    private static void ApplyCourierEditableFieldsOrThrow(BO.Courier target, BO.Courier source)
    {
        // Courier can update only: PhoneNumber, Email, MaxDist, ShippingMethod (with restrictions)
        // Courier cannot update: Id, FullName, IsActive, EmploymentStartDate, totals, OrderInProg

        // basic field validation (UI side)
        if (string.IsNullOrWhiteSpace(source.PhoneNumber))
            throw new ArgumentException("Phone Number is required.", nameof(source.PhoneNumber));

        if (string.IsNullOrWhiteSpace(source.Email))
            throw new ArgumentException("Email is required.", nameof(source.Email));

        if (source.MaxDist is null || source.MaxDist <= 0)
            throw new ArgumentException("Max Distance must be a positive number.", nameof(source.MaxDist));

        // Rule: MaxDist <= company max distance
        var cfg = s_bl.Admin.GetConfig();
        var companyMaxDist = cfg.MaxDist ?? double.MaxValue;
        if (source.MaxDist > companyMaxDist)
            throw new ArgumentException($"Max Distance cannot exceed company Max Distance ({companyMaxDist}).", nameof(source.MaxDist));

        // Rule: if courier has an active order, disallow shipping method change
        bool hasActiveOrder = (target.OrderInProg?.OrderId ?? 0) != 0;
        if (hasActiveOrder && target.ShippingMethod != source.ShippingMethod)
            throw new InvalidOperationException("Cannot change Shipping Method while an order is in progress.");

        // Apply allowed fields
        target.PhoneNumber = source.PhoneNumber;
        target.Email = source.Email;
        target.MaxDist = source.MaxDist;
        target.ShippingMethod = source.ShippingMethod;
    }

    private void btnSelectOrder_Click(object sender, RoutedEventArgs e)
    {
        if (!CanSelectOrder)
            return;

        try
        {
            new CourierOrderSelectionWindow(_courierId).ShowDialog();
            LoadCourierDetails();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error selecting order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnFinishHandling_Click(object sender, RoutedEventArgs e)
    {
        if (!CanFinishHandling || CurrentCourier is null)
            return;

        try
        {
            var deliveryId = CurrentCourier.OrderInProg.DeliveryId;

            // BL currently records Delivered regardless of selected type.
            // This UI allows finishing for any selected completion type.
            s_bl.Order.OrderComplete(_courierId, _courierId, deliveryId);

            MessageBox.Show("Order completed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadCourierDetails();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error finishing handling: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void btnDeliveryHistory_Click(object sender, RoutedEventArgs e)
    {
        new DeliveryHistoryWindow(_courierId).ShowDialog();
    }

    /// <summary>
    /// Recomputes the last completion type from courier delivery history.
    /// </summary>
    private void LoadLastCompletionType()
    {
        try
        {
            var history = s_bl.Order.GetCompletedCourierDeliveriesAsync(_courierId, _courierId, null, null)
                                  .ConfigureAwait(false)
                                  .GetAwaiter()
                                  .GetResult();

            // Take latest by DeliveryId (your DAL uses increasing IDs)
            var last = history
                .OrderByDescending(h => h.DeliveryId)
                .FirstOrDefault();

            LastCompletionTypeText = last?.CompletionType.ToString() ?? "N/A";
        }
        catch
        {
            LastCompletionTypeText = "N/A";
        }
    }
}
