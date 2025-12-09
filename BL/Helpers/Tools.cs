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

    internal static BO.OrderStatus? FindOrderStatusType(BO.OrderStatus? status)
    {
        return status switch
        {
            BO.OrderStatus.Open => BO.OrderStatus.Open,
            BO.OrderStatus.InProgress => BO.OrderStatus.InProgress,
            BO.OrderStatus.Completed => BO.OrderStatus.Completed,
            BO.OrderStatus.Rejected => BO.OrderStatus.Rejected,
            BO.OrderStatus.Cancelled => BO.OrderStatus.Cancelled,
            _ => null
        };
    }

    internal static BO.ScheduleStatus? SwitchScheduleStatusTOBO(BO.ScheduleStatus? status)
    {
        return status switch
        {
            BO.ScheduleStatus.OnTime => BO.ScheduleStatus.OnTime,
            BO.ScheduleStatus.InRisk => BO.ScheduleStatus.InRisk,
            BO.ScheduleStatus.Late => BO.ScheduleStatus.Late,
            _ => null
        };
    }
}

