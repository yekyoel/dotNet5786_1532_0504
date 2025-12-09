using BO;
using DalApi;
using DO;

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
        
        if (delivery == null || delivery.ShippingMethod == null)
            return null; // No delivery or no shipping method assigned yet
        
        // Get config for time calculations
        var config = AdminManager.GetConfig();
        if (config == null)
            return null;
        
        DateTime orderTime = order.StartTimeForOrdering ?? config.Clock;
        DateTime currentTime = config.Clock;
        
        TimeSpan maxDelTime = config.MaxDelTime;
        TimeSpan riskRange = config.RiskRange;
        
        DateTime expectedDeliveryTime = orderTime.Add(maxDelTime);
        DateTime riskThresholdTime = expectedDeliveryTime.Subtract(riskRange);
        
        // If delivery is completed, check if it was on time
        if (delivery.DeliveryEndTime.HasValue)
        {
            if (delivery.DeliveryEndTime.Value <= expectedDeliveryTime)
                return BO.ScheduleStatus.OnTime;
            else
                return BO.ScheduleStatus.Late;
        }
        
        // Delivery is still in progress - check current time against thresholds
        if (currentTime <= riskThresholdTime)
            return BO.ScheduleStatus.OnTime; // Still within safe window
        else if (currentTime <= expectedDeliveryTime)
            return BO.ScheduleStatus.InRisk; // Passed risk threshold but not yet late
        else
            return BO.ScheduleStatus.Late; // Already late
    }

    

}