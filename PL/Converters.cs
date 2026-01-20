using DO;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Collections.Generic;

namespace PL;

public class BoolToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts a Boolean value to a corresponding Visibility value.
    /// </summary>
    /// <remarks>This converter is typically used in data binding scenarios to control the visibility of UI
    /// elements based on a Boolean property value.</remarks>
    /// <param name="value">The value produced by the binding source. Expected to be a Boolean value.</param>
    /// <param name="targetType">The type of the binding target property. This parameter is not used.</param>
    /// <param name="parameter">An optional parameter to be used in the converter logic. This parameter is not used.</param>
    /// <param name="culture">The culture to use in the converter. This parameter is not used.</param>
    /// <returns>A Visibility value of Visibility.Visible if the input value is true; otherwise, Visibility.Collapsed.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool flag = value is bool b && b;
        return flag ? Visibility.Visible : Visibility.Collapsed;
    }

public class CanDeleteFromMapConverter : IValueConverter
{
    /// <summary>
    /// Converts an integer identifier to a Boolean value indicating whether deletion is allowed, based on a provided
    /// mapping.
    /// </summary>
    /// <remarks>If the value is not a positive integer or the parameter is not a valid mapping, the method
    /// returns <see langword="false"/>.</remarks>
    /// <param name="value">The value to convert. Expected to be an integer identifier greater than zero.</param>
    /// <param name="targetType">The type to convert the value to. This parameter is not used.</param>
    /// <param name="parameter">A mapping of integer identifiers to Boolean values indicating deletion permission. Must be a Dictionary<int,
    /// bool> if provided.</param>
    /// <param name="culture">The culture to use in the converter. This parameter is not used.</param>
    /// <returns>A Boolean value indicating whether the specified identifier can be deleted, as determined by the mapping.
    /// Returns <see langword="false"/> if the identifier is not valid or not found in the mapping.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not int id || id <= 0)
            return false;

        if (parameter is Dictionary<int, bool> map && map.TryGetValue(id, out var canDelete))
            return canDelete;

        return false;
    }

    /// <summary>
    /// Converts a value back to its source type. This method is typically used in data binding scenarios to convert
    /// values from the target back to the source.
    /// </summary>
    /// <param name="value">The value that is produced by the binding target and needs to be converted.</param>
    /// <param name="targetType">The type to convert the value to.</param>
    /// <param name="parameter">An optional parameter to be used in the conversion logic.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>The converted value to be passed to the source object.</returns>
    /// <exception cref="NotImplementedException">Always thrown, as this method is not implemented.</exception>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
    /// <summary>
    /// Converts a value back to its source type. This method is typically used in data binding scenarios to convert
    /// values from the target back to the source.
    /// </summary>
    /// <param name="value">The value that is produced by the binding target and needs to be converted.</param>
    /// <param name="targetType">The type to convert the value to.</param>
    /// <param name="parameter">An optional parameter to be used in the conversion logic.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>The converted value to be passed to the source object.</returns>
    /// <exception cref="NotImplementedException">This method is not implemented and always throws this exception.</exception>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

public class AddUpdateTextConverter : IValueConverter
{
    /// <summary>
    /// Converts a Boolean value to a corresponding action string, returning either "Add" or "Update" based on the
    /// input.
    /// </summary>
    /// <param name="value">The value to convert. If this value is a Boolean and is <see langword="true"/>, the method returns "Add";
    /// otherwise, it returns "Update".</param>
    /// <param name="targetType">The type to convert the value to. This parameter is not used in this implementation.</param>
    /// <param name="parameter">An optional parameter to be used in the conversion logic. This parameter is not used in this implementation.</param>
    /// <param name="culture">The culture to use in the converter. This parameter is not used in this implementation.</param>
    /// <returns>A string value: "Add" if <paramref name="value"/> is a Boolean and <see langword="true"/>; otherwise, "Update".</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isAdd = value is bool b && b;
        return isAdd ? "Add" : "Update";
    }

    /// <summary>
    /// Converts a value back to its source data type. This method is typically used in data binding scenarios to
    /// convert values from the target type back to the source type.
    /// </summary>
    /// <param name="value">The value that is produced by the binding target and needs to be converted back to the source type.</param>
    /// <param name="targetType">The type to convert the value to.</param>
    /// <param name="parameter">An optional parameter to be used in the conversion logic. This value can be null.</param>
    /// <param name="culture">The culture to use in the converter. This is typically used to format the conversion appropriately for the
    /// specified culture.</param>
    /// <returns>The converted value to be passed back to the source object.</returns>
    /// <exception cref="NotImplementedException">This method is not implemented and always throws this exception.</exception>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

public class CanCancelOrderConverter : IValueConverter
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    /// <summary>
    /// Determines whether an order is eligible for further processing based on its completion status.
    /// </summary>
    /// <remarks>If the order identifier is invalid or an error occurs while retrieving order details, the
    /// method returns <see langword="true"/> to indicate that the order is eligible for further processing by
    /// default.</remarks>
    /// <param name="value">The value representing the order identifier. Must be a positive integer.</param>
    /// <param name="targetType">The type to which the result should be converted. This parameter is not used.</param>
    /// <param name="parameter">An optional parameter for the conversion logic. This parameter is not used.</param>
    /// <param name="culture">The culture to use in the converter. This parameter is not used.</param>
    /// <returns>A Boolean value indicating whether the order can proceed to the next step. Returns <see langword="true"/> if the
    /// order is eligible for further processing; otherwise, <see langword="false"/>.</returns>
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

    /// <summary>
    /// Converts a value back to its source data type. This method is typically used in data binding scenarios to
    /// convert values from the target type back to the source type.
    /// </summary>
    /// <param name="value">The value that is produced by the binding target and needs to be converted.</param>
    /// <param name="targetType">The type to convert the value to.</param>
    /// <param name="parameter">An optional parameter to be used in the conversion logic.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>The converted value. The exact return type depends on the implementation.</returns>
    /// <exception cref="NotImplementedException">Always thrown, as this method is not implemented.</exception>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

public class CanDeleteCourierConverter : IValueConverter
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    /// <summary>
    /// Determines whether a courier can be deleted based on the presence of completed deliveries.
    /// </summary>
    /// <remarks>If the input is not a positive integer, or if an error occurs while checking for completed
    /// deliveries, the method returns <see langword="false"/> to prevent deletion.</remarks>
    /// <param name="value">The value to evaluate, expected to be an integer representing the courier ID.</param>
    /// <param name="targetType">The type to convert the value to. This parameter is not used.</param>
    /// <param name="parameter">An optional parameter to influence the conversion. This parameter is not used.</param>
    /// <param name="culture">The culture to use in the converter. This parameter is not used.</param>
    /// <returns>A Boolean value indicating whether the courier can be deleted. Returns <see langword="true"/> if the courier has
    /// no completed deliveries and the ID is valid; otherwise, <see langword="false"/>.</returns>
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

    /// <summary>
    /// Converts a value back to its source data type. This method is typically used in data binding scenarios to
    /// convert values from the target type back to the source type.
    /// </summary>
    /// <param name="value">The value that is produced by the binding target and needs to be converted back to the source type.</param>
    /// <param name="targetType">The type to convert the value to.</param>
    /// <param name="parameter">An optional parameter to be used in the conversion logic. This value can be null.</param>
    /// <param name="culture">The culture to use in the converter. This is typically used to format the conversion appropriately for the
    /// specified culture.</param>
    /// <returns>The converted value to be passed to the source object.</returns>
    /// <exception cref="NotImplementedException">This method is not implemented and always throws this exception.</exception>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}