using BO;
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
/// Interaction logic for CourierListWindow.xaml
/// </summary>
public partial class CourierListWindow : Window
{
    public CourierListWindow()
    {
        InitializeComponent();
    }

    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    public IEnumerable<BO.CourierInList> CourierList
    {
        get { return (IEnumerable<BO.CourierInList>)GetValue(CourseListProperty); }
        set { SetValue(CourseListProperty, value); }
    }

    public static readonly DependencyProperty CourseListProperty =
        DependencyProperty.Register("CourseList", typeof(IEnumerable<BO.CourierInList>), typeof(CourierListWindow), new PropertyMetadata(null));

    public BO.ShippingMethod FilterShippingMethods { get; set; } = BO.ShippingMethod.None;
}
