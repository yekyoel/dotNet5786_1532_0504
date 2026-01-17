using PL.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PL.Courier;

public partial class CourierWindow : Window
{
    // BL Static Reference properties
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
    private readonly ObserverMutex _mutex = new();
    
    // PL properties
    private readonly int _userId = s_bl.Admin.GetConfig().AdminId;
    bool _isObserverRegistered;
    private int _courierId;

    // dependency properties
    public static readonly DependencyProperty ButtonTextProperty =
        DependencyProperty.Register("ButtonText", typeof(string), typeof(CourierWindow));

    public static readonly DependencyProperty IsUpdateModeProperty =
       DependencyProperty.Register("IsUpdateMode", typeof(bool), typeof(CourierWindow), new PropertyMetadata(false));

    public static readonly DependencyProperty CurrentCourierProperty =
       DependencyProperty.Register("CurrentCourier", typeof(BO.Courier), typeof(CourierWindow), new PropertyMetadata(null));


    /// <summary>
    /// Gets or sets the text displayed on the action button (Add/Update).
    /// </summary>
    public string ButtonText
    {
        get { return (string)GetValue(ButtonTextProperty); }
        set { SetValue(ButtonTextProperty, value); }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the control is in update mode.
    /// </summary>
    public bool IsUpdateMode
    {
        get { return (bool)GetValue(IsUpdateModeProperty); }
        set { SetValue(IsUpdateModeProperty, value); }
    }

    /// <summary>
    /// Gets or sets the currently selected courier.
    /// </summary>
    public BO.Courier? CurrentCourier
    {
        get { return (BO.Courier?)GetValue(CurrentCourierProperty); }
        set { SetValue(CurrentCourierProperty, value); }
    }

   

    /// <summary>
    /// Initializes a new instance of the CourierWindow class for adding a new courier or updating an existing one.
    /// </summary>
    /// <remarks>If an error occurs while loading the courier details, an error message is displayed and the
    /// window is closed.</remarks>
    /// <param name="id">The identifier of the courier to update. If 0, a new courier is created; otherwise, the courier with the
    /// specified identifier is loaded for editing.</param>
    public CourierWindow(int id = 0)
    {
        ButtonText = id == 0 ? "Add" : "Update";
        IsUpdateMode = id != 0;
        _courierId = id;
        InitializeComponent();

        if(id == 0)
            CurrentCourier = new BO.Courier();
    }

    /// <summary>
    /// Handles the Loaded event of the window to perform initialization tasks when the window is first displayed.
    /// </summary>
    /// <param name="sender">The source of the event, typically the window being loaded.</param>
    /// <param name="e">The event data associated with the Loaded event.</param>
    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            if (IsUpdateMode == true)
            {
                CurrentCourier = await s_bl.Courier.GetCourierDetails(_userId, _courierId)!;
            }

            var courier = CurrentCourier;
            if (courier?.Id > 0)
            {
                s_bl.Courier.AddObserver(courier.Id, courierObserver);
                _isObserverRegistered = true;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading courier details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }
    }

    /// <summary>
    /// Handles the Closed event of the window and performs necessary cleanup operations.
    /// </summary>
    /// <remarks>This method is intended to be used as an event handler for the window's Closed event. It
    /// ensures that any observers associated with the current courier are properly removed when the window is
    /// closed.</remarks>
    /// <param name="sender">The source of the event, typically the window being closed.</param>
    /// <param name="e">An EventArgs object that contains the event data.</param>
    private void Window_Closed(object sender, EventArgs e)
    {
        if (!_isObserverRegistered)
            return;

        try
        {
            var courier = CurrentCourier;
            if (courier?.Id > 0)
                s_bl.Courier.RemoveObserver(courier.Id, courierObserver);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error while unsubscribing from updates: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Refreshes the details of the current courier by retrieving the latest information from the data source.
    /// </summary>
    /// <remarks>This method updates the CurrentCourier property with the most recent data for the currently
    /// selected courier. If CurrentCourier is null, a NullReferenceException will occur. This method is intended for
    /// internal use within the class to ensure that courier information remains up to date.</remarks>
    private void courierObserver()
    {
        _ = Dispatcher.BeginInvoke(async () =>
        {

            try
            {
                var courier = CurrentCourier;
                if (courier is null || courier.Id <= 0)
                    return;

                CurrentCourier = await s_bl.Courier.GetCourierDetails(_userId, courier.Id);
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                    MessageBox.Show($"Error refreshing courier details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error));
            }
            if (await _mutex.UnsetLoadInProgressAndCheckRestartRequested())
                courierObserver();

        });
    }


    /// <summary>
    /// Validates the courier data before adding or updating.
    /// </summary>
    private static void ValidateCourierOrThrow(BO.Courier courier)
    {
        if (courier is null)
            throw new ArgumentNullException(nameof(courier), "Courier cannot be null.");

        // ID validation (if adding, ID should be 0; if updating, ID should be positive)
        if (courier.Id < 100000000 || courier.Id > 999999999)
            throw new ArgumentException("Courier ID needs to have 9 digits", nameof(courier.Id));

        // Full Name
        if (string.IsNullOrWhiteSpace(courier.FullName))
            throw new ArgumentException("Full Name is required.", nameof(courier.FullName));

        // Phone Number Validations
        if (string.IsNullOrWhiteSpace(courier.PhoneNumber))
            throw new ArgumentException("Phone Number is required.", nameof(courier.PhoneNumber));

        if (!courier.PhoneNumber.All(char.IsDigit))
            throw new ArgumentException("Phone Number must contain only digits.", nameof(courier.PhoneNumber));

        if (courier.PhoneNumber.Length != 10)
            throw new ArgumentException("Phone Number must be exactly 10 digits long.", nameof(courier.PhoneNumber));

        if (!courier.PhoneNumber.StartsWith("05"))
            throw new ArgumentException("Phone Number must start with '05'.", nameof(courier.PhoneNumber));

        // Email
        if (string.IsNullOrWhiteSpace(courier.Email))
            throw new ArgumentException("Email is required.", nameof(courier.Email));

        // Max Distance
        if (courier.MaxDist is null || courier.MaxDist <= 0)
            throw new ArgumentException("Max Distance must be a positive number.", nameof(courier.MaxDist));

        // Employment Start Date
        if (courier.EmploymentStartDate is null)
            throw new ArgumentException("Employment Start Date is required.", nameof(courier.EmploymentStartDate));

        // Shipping Method
        if (courier.ShippingMethod is null || courier.ShippingMethod == BO.ShippingMethod.None)
            throw new ArgumentException("Shipping Method is required.", nameof(courier.ShippingMethod));
    }


    /// <summary>
    /// Handles both adding and updating courier information.
    /// </summary>
    private void btnAddUpdate_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Validate courier before sending to BL
            ValidateCourierOrThrow(CurrentCourier!);

            if (ButtonText == "Add")
            {
                s_bl.Courier.AddCourier(123456789, CurrentCourier!);
                MessageBox.Show("Courier added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            else
            {
                s_bl.Courier.UpdateCourierDetails(123456789, CurrentCourier!);
                MessageBox.Show("Courier updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Closes the courier window.
    /// </summary>
    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}