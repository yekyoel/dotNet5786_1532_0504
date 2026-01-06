using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PL;

public class BoolToVisibilityConverter : IValueConverter
{
    // parameter = "Invert" to invert logic
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool flag = value is bool b && b;
        bool invert = string.Equals(parameter as string, "Invert", StringComparison.OrdinalIgnoreCase);

        if (invert)
            flag = !flag;

        return flag ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

public class AddUpdateTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isAdd = value is bool b && b;
        return isAdd ? "Add" : "Update";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class CanCancelOrderConverter : IValueConverter
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not int orderId || orderId <= 0)
            return false;

        try
        {
            // Same logic as GetCompletionTypeForOrder + CanCancel
            var userId = s_bl.Admin.GetConfig().AdminId;
            var order = s_bl.Order.GetOrderDetails(userId, orderId);

            var type = order.DeliveriesList?
                .OrderByDescending(d => d.DeliveryId)
                .FirstOrDefault()
                ?.CompType;

            // Only allow cancel if not already Cancelled or Delivered
            return type != BO.CompletionType.Cancelled
                   && type != BO.CompletionType.Delivered
                   && type != BO.CompletionType.Failed; 
        }
        catch
        {
            // On error, be safe and disable cancel
            return false;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}