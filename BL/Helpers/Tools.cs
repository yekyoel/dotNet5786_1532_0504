using DalApi;

namespace Helpers;

internal static class Tools
{

    //public static string ToStringProperty<T>(this T t) { }

    internal static bool checkProperty<T>(T t) => t is not null;

    internal static object? GetProperty(object obj, string propertyName)
    {
        return obj.GetType().GetProperty(propertyName)?
                  .GetValue(obj, null);
    }

    internal static BO.OrderType? SwitchOrderTypeTOBO(DO.Order order)
    {
        return order.Food switch
        {
            DO.OrderType.Pizza => BO.OrderType.Pizza,
            DO.OrderType.Hamburger => BO.OrderType.Hamburger,
            DO.OrderType.Fries => BO.OrderType.Fries,
            DO.OrderType.IceCream => BO.OrderType.IceCream,
            _ => null
        };
    }

    internal static DO.OrderType? SwitchOrderTypeTODO(BO.Order order)
    {
        return order.OrderTyype switch
        {
            BO.OrderType.Pizza => DO.OrderType.Pizza,
            BO.OrderType.Hamburger => DO.OrderType.Hamburger,
            BO.OrderType.Fries => DO.OrderType.Fries,
            BO.OrderType.IceCream => DO.OrderType.IceCream,
            _ => null
        };
    }


    internal static BO.OrderStatus? FindOrderStatusType(DO.Order order)
    {
        // Check if delivery exists for this order
        var delivery = DeliveryManager.GetDeliveryByOrderId(order.Id);
        
        if (delivery == null)
            return BO.OrderStatus.Open; // No delivery = Open
        
        if (delivery.ShippingMethod == null)
            return BO.OrderStatus.InProgress; // Delivery exists but not assigned yet
        
        // Check the completion type for final statuses
        return delivery.End switch
        {
            DO.CompletionType.Pending => BO.OrderStatus.InProgress,
            DO.CompletionType.Refused => BO.OrderStatus.Rejected,
            DO.CompletionType.Delivered => BO.OrderStatus.Completed,
            DO.CompletionType.Cancelled => BO.OrderStatus.Cancelled,
            DO.CompletionType.Failed => BO.OrderStatus.Rejected,
            null => BO.OrderStatus.InProgress,
            _ => BO.OrderStatus.InProgress
        };
    }

    internal static BO.ScheduleStatus? FindScheduleStatusType(DO.Order order)
    {
        var delivery = DeliveryManager.GetDeliveryByOrderId(order.Id);
        
        if (delivery?.DeliveryStartTime == null)
            return null; // No delivery start time yet
        
        if (delivery?.DeliveryEndTime == null)
            return null; // Still in progress, can't determine schedule status
        
        // Determine if on time, in risk, or late based on delivery duration
        // This is a simple implementation - you may need to use config values for actual calculation
        if (delivery.End == DO.CompletionType.Delivered)
            return BO.ScheduleStatus.OnTime; // delivered = on time
        else if (delivery.End == DO.CompletionType.Pending || delivery.End == DO.CompletionType.Refused)
            return BO.ScheduleStatus.InRisk; // pending or refused = in risk
        else if (delivery.End == DO.CompletionType.Failed)
            return BO.ScheduleStatus.Late; // failed = late
        
        return null;
    }

}

