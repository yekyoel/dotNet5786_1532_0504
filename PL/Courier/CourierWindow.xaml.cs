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
        ButtonText = id == 0 ? "Add" : "Update";
        IsUpdateMode = id != 0;
        InitializeComponent();
        
        try
        {
            CurrentCourier = (id != 0) ? s_bl.Courier.GetCourierDetails(123456789, id)! : new BO.Courier();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading courier: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            this.Close();
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (CurrentCourier?.Id != 0)
            s_bl.Courier.AddObserver(CurrentCourier!.Id, courierObserver);
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        if (CurrentCourier?.Id != 0)
            s_bl.Courier.RemoveObserver(CurrentCourier!.Id, courierObserver);
    }

    private void courierObserver()
    {
        int id = CurrentCourier!.Id;
        CurrentCourier = null;
        CurrentCourier = s_bl.Courier.GetCourierDetails(123456789, id);
    }


    public static readonly DependencyProperty ButtonTextProperty =
        DependencyProperty.Register("ButtonText", typeof(string), typeof(CourierWindow));

    public string ButtonText
    {
        get { return (string)GetValue(ButtonTextProperty); }
        set { SetValue(ButtonTextProperty, value); }
    }

    public bool IsUpdateMode
    {
        get { return (bool)GetValue(IsUpdateModeProperty); }
        set { SetValue(IsUpdateModeProperty, value); }
    }

    public static readonly DependencyProperty IsUpdateModeProperty =
        DependencyProperty.Register("IsUpdateMode", typeof(bool), typeof(CourierWindow), new PropertyMetadata(false));


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
            if (ButtonText == "Add")
            {
                s_bl.Courier.AddCourier(123456789, CurrentCourier!);
                MessageBox.Show("Courier added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                s_bl.Courier.UpdateCourierDetails(123456789, CurrentCourier!);
                MessageBox.Show("Courier updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
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
}