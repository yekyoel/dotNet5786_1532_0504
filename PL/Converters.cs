using DO;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace PL;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool flag = value is bool b && b;
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

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
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
            int userId = s_bl.Admin.GetConfig().AdminId;
            var order = s_bl.Order.GetOrderDetails(userId, orderId);

            var lastDelivery = order.DeliveriesList?
                .OrderByDescending(d => d.DeliveryId)
                .FirstOrDefault();

            BO.CompletionType? type = lastDelivery?.CompType;

            if (type is null)
                return true;

            return type != BO.CompletionType.Cancelled
                   && type != BO.CompletionType.Delivered
                   && type != BO.CompletionType.Refused
                   && type != BO.CompletionType.Failed;
        }
        catch
        {
            return true;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

public class CanDeleteCourierConverter : IValueConverter
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not int courierId || courierId <= 0)
            return false;

        try
        {
            int userId = s_bl.Admin.GetConfig().AdminId;
            // If courier has any closed deliveries, do not allow delete
            var deliveries = s_bl.Order.GetCompletedCourierDeliveriesAsync(userId, courierId, null, null)
                                     .ConfigureAwait(false)
                                     .GetAwaiter()
                                     .GetResult();
            bool hasAny = deliveries != null && deliveries.Any();
            return !hasAny;
        }
        catch
        {
            // On error, be conservative and disallow delete
            return false;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}