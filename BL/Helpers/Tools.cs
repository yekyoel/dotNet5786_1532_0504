using BO;
using DalApi;
using DO;

namespace Helpers;

internal static class Tools
{

    public static string ToStringProperty<T>(this T t) 
    { 
        return t?.ToString() ?? "null";
    }

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
            _ => null
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

    internal static double GetAerialDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371.0; // Earth radius in km
        static double ToRad(double deg) => deg * Math.PI / 180.0;

        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    internal static double GetAerialDistanceFromStoreKm(DO.Order order)
    {
        var cfg = AdminManager.GetConfig();
        var storeLat = cfg?.Latitude ?? 0.0;
        var storeLon = cfg?.Longitude ?? 0.0;
        return GetAerialDistanceKm(storeLat, storeLon, order.Latitude, order.Longitude);
    }

    // Returns the most-frequent OrderType (food) for a courier.
    // If the courier has no deliveries/orders, returns a sensible default (Pizza).
    internal static BO.OrderType FindCourierOrderType(DO.Courier courier)
    {
        if (courier is null)
            throw new ArgumentNullException(nameof(courier)); // throw an exception if courier is null

        var dal = Factory.Get; // get DAL instance
        // read deliveries assigned to this courier
        var deliveries = dal.Delivery.ReadAll(d => d.CourierId == courier.Id);

        // count foods by DO.OrderType
        var counts = new Dictionary<DO.OrderType, int>();
        foreach (var del in deliveries)
        {
            // safe read of order (Read returns Order? in DAL implementations)
            var order = dal.Order.Read(del.OrderId);
            if (order?.Food is DO.OrderType ot)
            {
                if (counts.ContainsKey(ot)) counts[ot]++; else counts[ot] = 1;
            }
        }

        if (counts.Count == 0)
        {
            // no history -> choose default
            return BO.OrderType.Pizza;
        }

        var mostFrequent = counts.OrderByDescending(kv => kv.Value).First().Key; // get most frequent DO.OrderType

        // map DO.OrderType to BO.OrderType and return
        return mostFrequent switch
        {
            DO.OrderType.Pizza => BO.OrderType.Pizza,
            DO.OrderType.Hamburger => BO.OrderType.Hamburger,
            DO.OrderType.Fries => BO.OrderType.Fries,
            DO.OrderType.IceCream => BO.OrderType.IceCream,
            _ => BO.OrderType.Pizza
        };
    }

    // Map DO.Courier.PreferredShippingMethod (nullable DO.ShippingMethod?) to BO.ShippingMethod?
    internal static BO.ShippingMethod? FindType(DO.Courier courier)
    {
        if (courier is null)
            throw new ArgumentNullException(nameof(courier));

        var doMethod = courier.PreferredShippingMethod;

        if (doMethod is null)
            return null;

        return doMethod.Value switch
        {
            DO.ShippingMethod.Car => BO.ShippingMethod.Car,
            DO.ShippingMethod.Motorcycle => BO.ShippingMethod.Motorcycle,
            DO.ShippingMethod.Bike => BO.ShippingMethod.Bike,
            DO.ShippingMethod.OnFoot => BO.ShippingMethod.OnFoot,
            _ => null
        };
    }

    internal static BO.ShippingMethod? SwitchShippingMethodTOBO(DO.ShippingMethod? shippingMethod)
    {
        return shippingMethod switch
        {
            DO.ShippingMethod.Car => BO.ShippingMethod.Car,
            DO.ShippingMethod.Motorcycle => BO.ShippingMethod.Motorcycle,
            DO.ShippingMethod.Bike => BO.ShippingMethod.Bike,
            DO.ShippingMethod.OnFoot => BO.ShippingMethod.OnFoot,
            null => null,
            _ => null
        };
    }

    internal static BO.CompletionType? SwitchCompletionTypeTOBO(DO.CompletionType? completionType)
    {
        return completionType switch
        {
            DO.CompletionType.Pending => BO.CompletionType.Pending,
            DO.CompletionType.Refused => BO.CompletionType.Refused,
            DO.CompletionType.Delivered => BO.CompletionType.Delivered,
            DO.CompletionType.Cancelled => BO.CompletionType.Cancelled,
            DO.CompletionType.Failed => BO.CompletionType.Failed,
            null => null,
            _ => null
        };
    }
}