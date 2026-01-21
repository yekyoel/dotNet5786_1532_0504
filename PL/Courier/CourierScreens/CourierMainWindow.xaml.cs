using PL.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace PL.Courier.CourierScreens;

public partial class CourierMainWindow : Window
{
    // BL Static Reference properties
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
    private readonly ObserverMutex _mutex = new();

    // PL properties
    private readonly int _courierId;
    private bool _isObserverRegistered;

    public static readonly DependencyProperty CurrentCourierProperty =
        DependencyProperty.Register(nameof(CurrentCourier), typeof(BO.Courier), typeof(CourierMainWindow), new PropertyMetadata(null));

    public BO.Courier? CurrentCourier
    {
        get => (BO.Courier?)GetValue(CurrentCourierProperty);
        set => SetValue(CurrentCourierProperty, value);
    }

    public static readonly DependencyProperty LastCompletionTypeTextProperty =
        DependencyProperty.Register(nameof(LastCompletionTypeText), typeof(string), typeof(CourierMainWindow), new PropertyMetadata("N/A"));

    public string LastCompletionTypeText
    {
        get => (string)GetValue(LastCompletionTypeTextProperty);
        set => SetValue(LastCompletionTypeTextProperty, value);
    }

    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(CourierMainWindow), new PropertyMetadata(false));

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public static readonly DependencyProperty SelectedCompletionTypeProperty =
        DependencyProperty.Register(nameof(SelectedCompletionType), typeof(BO.CompletionType), typeof(CourierMainWindow), new PropertyMetadata(BO.CompletionType.None));

    public BO.CompletionType SelectedCompletionType
    {
        get => (BO.CompletionType)GetValue(SelectedCompletionTypeProperty);
        set => SetValue(SelectedCompletionTypeProperty, value);
    }

    public static readonly DependencyPropertyKey CompletionTypesKey =
        DependencyProperty.RegisterReadOnly(nameof(CompletionTypes), typeof(IEnumerable<BO.CompletionType>), typeof(CourierMainWindow), new PropertyMetadata(Enum.GetValues<BO.CompletionType>()));
    public static readonly DependencyProperty CompletionTypesProperty = CompletionTypesKey.DependencyProperty;

    public IEnumerable<BO.CompletionType> CompletionTypes => (IEnumerable<BO.CompletionType>)GetValue(CompletionTypesProperty);

    // Helper wrappers for logic properties that depend on CurrentCourier
    private static readonly DependencyPropertyKey HasOrderInProgressKey =
        DependencyProperty.RegisterReadOnly(nameof(HasOrderInProgress), typeof(bool), typeof(CourierMainWindow), new PropertyMetadata(false));
    public static readonly DependencyProperty HasOrderInProgressProperty = HasOrderInProgressKey.DependencyProperty;
    public bool HasOrderInProgress => (bool)GetValue(HasOrderInProgressProperty);

    private static readonly DependencyPropertyKey HasNoOrderInProgressKey =
        DependencyProperty.RegisterReadOnly(nameof(HasNoOrderInProgress), typeof(bool), typeof(CourierMainWindow), new PropertyMetadata(true));
    public static readonly DependencyProperty HasNoOrderInProgressProperty = HasNoOrderInProgressKey.DependencyProperty;
    public bool HasNoOrderInProgress => (bool)GetValue(HasNoOrderInProgressProperty);

    private static readonly DependencyPropertyKey CanSelectOrderKey =
        DependencyProperty.RegisterReadOnly(nameof(CanSelectOrder), typeof(bool), typeof(CourierMainWindow), new PropertyMetadata(false));
    public static readonly DependencyProperty CanSelectOrderProperty = CanSelectOrderKey.DependencyProperty;
    public bool CanSelectOrder => (bool)GetValue(CanSelectOrderProperty);

    private static readonly DependencyPropertyKey CanFinishHandlingKey =
        DependencyProperty.RegisterReadOnly(nameof(CanFinishHandling), typeof(bool), typeof(CourierMainWindow), new PropertyMetadata(false));
    public static readonly DependencyProperty CanFinishHandlingProperty = CanFinishHandlingKey.DependencyProperty;
    public bool CanFinishHandling => (bool)GetValue(CanFinishHandlingProperty);

    public CourierMainWindow(int courierId)
    {
        InitializeComponent();
        _courierId = courierId;

        //LoadCourierDetails();
       // DataContext = this;
    }

    
    /// <summary>
    /// Updates all dependent read-only properties based on CurrentCourier state.
    /// </summary>
    private void UpdateDependentProperties()
    {
        bool hasOrder = (CurrentCourier?.OrderInProg?.OrderId ?? 0) != 0;
        bool isActive = CurrentCourier?.IsActive ?? false;
        bool canFinish = hasOrder && SelectedCompletionType != BO.CompletionType.None;

        SetValue(HasOrderInProgressKey, hasOrder);
        SetValue(HasNoOrderInProgressKey, !hasOrder);
        SetValue(CanSelectOrderKey, CurrentCourier != null && isActive && !hasOrder);
        SetValue(CanFinishHandlingKey, canFinish);
    }

    private void CompletionType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        UpdateDependentProperties();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await LoadCourierDetailsAsync();
            s_bl.Courier.AddObserver(_courierId, CourierObserver);
            // Also listen to order list updates (clock changes affect schedule/availability)
            s_bl.Order.AddObserver(CourierObserver);
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
            s_bl.Order.RemoveObserver(CourierObserver);
        }
        catch( Exception ex)
        {
            MessageBox.Show($"Error while unsubscribing from updates: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CourierObserver()
    {
        if (_mutex.CheckAndSetLoadInProgressOrRestartRequired())
            return;
        _ = Dispatcher.BeginInvoke(async () =>
        {
            await LoadCourierDetailsAsync();

            if (await _mutex.UnsetLoadInProgressAndCheckRestartRequested())
                CourierObserver();

        });
    }

    private async Task LoadCourierDetailsAsync()
    {
        CurrentCourier = await s_bl.Courier.GetCourierDetails(_courierId, _courierId);

        // Reset completion choice when there is no order in progress (so button stays disabled).
        if (!HasOrderInProgress)
            SelectedCompletionType = BO.CompletionType.None;

        LoadLastCompletionType();
        UpdateDependentProperties();
    }



    private async void btnUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentCourier is null)
            return;

        try
        {
            // Always re-read authoritative record, then copy only allowed fields.
            var authoritative = await s_bl.Courier.GetCourierDetails(_courierId, _courierId);

            ApplyCourierEditableFieldsOrThrow(authoritative, CurrentCourier);

            s_bl.Courier.UpdateCourierDetails(_courierId, authoritative);
            MessageBox.Show("Details updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            await LoadCourierDetailsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Update Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            await LoadCourierDetailsAsync(); // revert UI to authoritative state
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
        target.FullName = source.FullName; 
        target.PhoneNumber = source.PhoneNumber;
        target.Email = source.Email;
        target.MaxDist = source.MaxDist;
        target.ShippingMethod = source.ShippingMethod;
    }

    private async void btnSelectOrder_Click(object sender, RoutedEventArgs e)
    {
        if (!CanSelectOrder)
            return;

        try
        {
            new CourierOrderSelectionWindow(_courierId).ShowDialog();
            await LoadCourierDetailsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error selecting order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void btnFinishHandling_Click(object sender, RoutedEventArgs e)
    {
        if (!CanFinishHandling || CurrentCourier is null)
            return;

        try
        {
            var deliveryId = CurrentCourier.OrderInProg.DeliveryId;

            // Updated to use the selected completion type
            s_bl.Order.OrderComplete(_courierId, _courierId, deliveryId, SelectedCompletionType);

            MessageBox.Show("Order completed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            await LoadCourierDetailsAsync();
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
    private async void LoadLastCompletionType()
    {
        try
        {
            IsLoading = true; 
            var history = await s_bl.Order.GetCompletedCourierDeliveriesAsync(_courierId, _courierId, null, null);

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
        finally
        {
            IsLoading = false;
        }
    }
}
