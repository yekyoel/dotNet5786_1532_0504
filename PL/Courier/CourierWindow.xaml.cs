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

/// <summary>
/// Interaction logic for CourierWindow.xaml
/// </summary>
public partial class CourierWindow : Window
{
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    public CourierWindow(int id = 0)
    {
        _courierId = id;
        ButtonText = id == 0 ? "Add" : "Update";
        InitializeComponent();
        // 9b . 5 idk what to do there 
    
    }

    #region Dependency Properties for Courier Bindings

    public static readonly DependencyProperty CourierIdProperty =
        DependencyProperty.Register("CourierId", typeof(int), typeof(CourierWindow));

    public int CourierId
    {
        get { return (int)GetValue(CourierIdProperty); }
        set { SetValue(CourierIdProperty, value); }
    }

    public static readonly DependencyProperty FullNameProperty =
        DependencyProperty.Register("FullName", typeof(string), typeof(CourierWindow));

    public string FullName
    {
        get { return (string)GetValue(FullNameProperty); }
        set { SetValue(FullNameProperty, value); }
    }

    public static readonly DependencyProperty PhoneNumberProperty =
        DependencyProperty.Register("PhoneNumber", typeof(string), typeof(CourierWindow));

    public string PhoneNumber
    {
        get { return (string)GetValue(PhoneNumberProperty); }
        set { SetValue(PhoneNumberProperty, value); }
    }

    public static readonly DependencyProperty EmailProperty =
        DependencyProperty.Register("Email", typeof(string), typeof(CourierWindow));

    public string Email
    {
        get { return (string)GetValue(EmailProperty); }
        set { SetValue(EmailProperty, value); }
    }


    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register("IsActive", typeof(bool), typeof(CourierWindow));

    public bool IsActive
    {
        get { return (bool)GetValue(IsActiveProperty); }
        set { SetValue(IsActiveProperty, value); }
    }

    public static readonly DependencyProperty MaxDistProperty =
        DependencyProperty.Register("MaxDist", typeof(double?), typeof(CourierWindow));

    public double? MaxDist
    {
        get { return (double?)GetValue(MaxDistProperty); }
        set { SetValue(MaxDistProperty, value); }
    }

    public static readonly DependencyProperty ShippingMethodProperty =
        DependencyProperty.Register("ShippingMethod", typeof(BO.ShippingMethod?), typeof(CourierWindow));

    public BO.ShippingMethod? ShippingMethod
    {
        get { return (BO.ShippingMethod?)GetValue(ShippingMethodProperty); }
        set { SetValue(ShippingMethodProperty, value); }
    }

    public static readonly DependencyProperty EmploymentStartDateProperty =
        DependencyProperty.Register("EmploymentStartDate", typeof(DateTime?), typeof(CourierWindow));

    public DateTime? EmploymentStartDate
    {
        get { return (DateTime?)GetValue(EmploymentStartDateProperty); }
        set { SetValue(EmploymentStartDateProperty, value); }
    }

    public static readonly DependencyProperty TotalDelSuppliedOnTimeProperty =
        DependencyProperty.Register("TotalDelSuppliedOnTime", typeof(int), typeof(CourierWindow));

    public int TotalDelSuppliedOnTime
    {
        get { return (int)GetValue(TotalDelSuppliedOnTimeProperty); }
        set { SetValue(TotalDelSuppliedOnTimeProperty, value); }
    }

    public static readonly DependencyProperty TotalDelSuppliedLateProperty =
        DependencyProperty.Register("TotalDelSuppliedLate", typeof(int), typeof(CourierWindow));

    public int TotalDelSuppliedLate
    {
        get { return (int)GetValue(TotalDelSuppliedLateProperty); }
        set { SetValue(TotalDelSuppliedLateProperty, value); }
    }

    public static readonly DependencyProperty ButtonTextProperty =
        DependencyProperty.Register("ButtonText", typeof(string), typeof(CourierWindow));

    public string ButtonText
    {
        get { return (string)GetValue(ButtonTextProperty); }
        set { SetValue(ButtonTextProperty, value); }
    }

    private int _courierId;

    #endregion

    public BO.Courier? CurrentCourier
    {
        get { return (BO.Courier?)GetValue(CurrentCourierProperty); }
        set { SetValue(CurrentCourierProperty, value); }
    }

    public static readonly DependencyProperty CurrentCourierProperty =
        DependencyProperty.Register("CurrentCourier", typeof(BO.Courier), typeof(CourierWindow), new PropertyMetadata(null));

    /// <summary>
    /// Handles both adding and updating courier information.
    /// </summary>
    private void btnAddUpdate_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var courier = new BO.Courier
            {
                Id = CourierId,
                FullName = FullName,
                PhoneNumber = PhoneNumber,
                Email = Email,
                IsActive = IsActive,
                MaxDist = MaxDist,
                ShippingMethod = ShippingMethod,
                EmploymentStartDate = EmploymentStartDate,
                TotalDelSuppliedOnTime = TotalDelSuppliedOnTime,
                TotalDelSuppliedLate = TotalDelSuppliedLate
            };

            if (_courierId == 0)
            {
                s_bl.Courier.AddCourier(0, courier);
                MessageBox.Show("Courier added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
            }
            else
            {
                s_bl.Courier.UpdateCourierDetails(0, courier);
                MessageBox.Show("Courier updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
        this.Close();
    }

    /// <summary>
    /// Clears all form fields.
    /// </summary>
    private void ClearForm()
    {
        CourierId = 0;
        FullName = string.Empty;
        PhoneNumber = string.Empty;
        Email = string.Empty;
        IsActive = false;
        MaxDist = null;
        ShippingMethod = null;
        EmploymentStartDate = null;
        TotalDelSuppliedOnTime = 0;
        TotalDelSuppliedLate = 0;
    }
}
