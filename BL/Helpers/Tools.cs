using BO;
using DalApi;
using DO;

namespace Helpers;

internal static class Tools
{
    /// <summary>
    /// Returns the string representation of the specified object, or "null" if the object is null.
    /// </summary>
    /// <typeparam name="T">The type of the object to convert to a string.</typeparam>
    /// <param name="t">The object to convert to its string representation. Can be null.</param>
    /// <returns>A string that represents the object, or "null" if the object is null.</returns>
    public static string ToStringProperty<T>(this T t) 
    { 
        return t?.ToString() ?? "null";
    }

    /// <summary>
    /// Converts the order type of the specified data object order to its corresponding business object order type.
    /// </summary>
    /// <param name="order">The data object order whose order type is to be converted.</param>
    /// <returns>The corresponding business object order type if a mapping exists; otherwise, null.</returns>
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

    /// <summary>
    /// Converts the specified business order type to its corresponding data order type, if a mapping exists.
    /// </summary>
    /// <remarks>This method returns null if the order type is not supported or does not have a direct
    /// mapping.</remarks>
    /// <param name="order">The business order to convert. Must not be null.</param>
    /// <returns>The corresponding data order type if the business order type is recognized; otherwise, null.</returns>
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

    /// <summary>
    /// Determines the business order status for the specified order based on its associated delivery and completion
    /// state.
    /// </summary>
    /// <remarks>The returned status reflects the delivery assignment and completion state. If no delivery is
    /// associated with the order, the status is considered open. If a delivery exists but has not been assigned a
    /// shipping method, the status is in progress. Final statuses are determined by the delivery's completion
    /// type.</remarks>
    /// <param name="order">The order for which to determine the business order status.</param>
    /// <returns>A value of <see cref="BO.OrderStatus"/> representing the current status of the order, or <see langword="null"/>
    /// if the status cannot be determined.</returns>
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

    /// <summary>
    /// Determines the schedule status of the specified order based on its delivery progress and configured time
    /// thresholds.
    /// </summary>
    /// <remarks>The schedule status is determined by comparing the order's delivery completion time and the
    /// current time against configured maximum delivery and risk time windows. If the delivery is completed, the status
    /// reflects whether it was on time or late. If the delivery is still in progress, the status indicates whether it
    /// is on time, in a risk period, or late based on the current time.</remarks>
    /// <param name="order">The order for which to determine the schedule status. Must not be null.</param>
    /// <returns>A value indicating the schedule status of the order: OnTime, InRisk, or Late. Returns null if the order does not
    /// have an associated delivery, shipping method, or configuration.</returns>
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

    /// <summary>
    /// Calculates the great-circle distance, in kilometers, between two geographic coordinates specified by latitude
    /// and longitude.
    /// </summary>
    /// <remarks>This method uses the Haversine formula to account for the Earth's curvature. The result
    /// assumes a spherical Earth and may not account for local variations in terrain or elevation.</remarks>
    /// <param name="lat1">The latitude of the first point, in decimal degrees.</param>
    /// <param name="lon1">The longitude of the first point, in decimal degrees.</param>
    /// <param name="lat2">The latitude of the second point, in decimal degrees.</param>
    /// <param name="lon2">The longitude of the second point, in decimal degrees.</param>
    /// <returns>The shortest distance between the two points, measured along the surface of the Earth, in kilometers.</returns>
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

    /// <summary>
    /// Calculates the straight-line distance, in kilometers, between the store location and the specified order's
    /// delivery location.
    /// </summary>
    /// <param name="order">The order for which to calculate the aerial distance from the store. Must have valid latitude and longitude
    /// coordinates.</param>
    /// <returns>The aerial distance in kilometers between the store and the order's delivery location.</returns>
    internal static double GetAerialDistanceFromStoreKm(DO.Order order)
    {
        var cfg = AdminManager.GetConfig();
        var storeLat = cfg?.Latitude ?? 0.0;
        var storeLon = cfg?.Longitude ?? 0.0;
        return GetAerialDistanceKm(storeLat, storeLon, order.Latitude, order.Longitude);
    }

   /// <summary>
   /// Maps the preferred shipping method of the specified courier to its corresponding business object shipping method.
   /// </summary>
   /// <param name="courier">The courier whose preferred shipping method is to be mapped. Cannot be null.</param>
   /// <returns>A value of <see cref="BO.ShippingMethod"/> corresponding to the courier's preferred shipping method, or <see
   /// langword="null"/> if the preferred shipping method is not set or cannot be mapped.</returns>
   /// <exception cref="ArgumentNullException">Thrown if <paramref name="courier"/> is null.</exception>
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

    /// <summary>
    /// Converts a shipping method value from the data object (DO) layer to its corresponding business object (BO)
    /// representation.
    /// </summary>
    /// <param name="shippingMethod">The shipping method value from the data object layer to convert. Can be null.</param>
    /// <returns>The corresponding shipping method value in the business object layer, or null if the input is null or does not
    /// match a known value.</returns>
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

    /// <summary>
    /// Converts a value of type DO.CompletionType to the corresponding BO.CompletionType value.
    /// </summary>
    /// <param name="completionType">The completion type value to convert. Can be null.</param>
    /// <returns>A BO.CompletionType value that corresponds to the specified DO.CompletionType value, or null if the input is
    /// null or does not match a known value.</returns>
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

    /// <summary>
    /// Calculates expected delivery time based on order placement, distance, and courier shipping method.
    /// Formula: OrderPlacedTime + DeliveryDuration + 10 min buffer
    /// DeliveryDuration is calculated using distance and speed based on shipping method.
    /// </summary>
    internal static DateTime? CalculateExpectedDeliveryTime(DO.Order order, DO.Delivery? delivery)
    {
        if (order == null)
            return null;

        var config = AdminManager.GetConfig();
        if (config == null)
            return null;

        // If no delivery or no shipping method assigned yet, cannot calculate
        if (delivery?.ShippingMethod == null)
            return null;

        // Order placement time
        DateTime orderTime = order.StartTimeForOrdering ?? config.Clock;

        // Distance in km (if not recorded, estimate from store to delivery location)
        double distanceKm = delivery.Distance ?? GetAerialDistanceFromStoreKm(order);

        // Get speed in mph based on shipping method, convert to kmh
        double speedMph = delivery.ShippingMethod.Value switch
        {
            DO.ShippingMethod.Car => config.AvgCarMPH,
            DO.ShippingMethod.Motorcycle => config.AvgMotorcycleMPH,
            DO.ShippingMethod.Bike => config.AvgBicycleMPH,
            DO.ShippingMethod.OnFoot => config.AvgWalkMPH,
            _ => 70.0
        };

        // Convert mph to kmh: mph * 1.60934
        double speedKmh = speedMph * 1.60934;
        if (speedKmh <= 0) speedKmh = 30.0;

        // Calculate delivery duration in hours
        double durationHours = distanceKm / speedKmh;

        // Expected time = order time + duration + 10 min buffer
        TimeSpan deliverySpan = TimeSpan.FromHours(durationHours);
        TimeSpan bufferSpan = TimeSpan.FromMinutes(10);

        return orderTime.Add(deliverySpan).Add(bufferSpan);
    }

    /// <summary>
    /// Calculates remaining time until max delivery deadline.
    /// TotalTimeLeft = MaxDeliveryTime - CurrentTime
    /// If negative or delivery complete, returns zero.
    /// </summary>
    internal static TimeSpan CalculateTotalTimeLeft(DO.Order order, DO.Delivery? delivery)
    {
        var config = AdminManager.GetConfig();
        if (config == null)
            return TimeSpan.Zero;

        // If delivery is complete, no time left
        if (delivery?.DeliveryEndTime != null)
            return TimeSpan.Zero;

        // Order placement time
        DateTime orderTime = order.StartTimeForOrdering ?? config.Clock;

        // Max delivery deadline = order time + config max delivery time
        DateTime maxDeliveryDeadline = orderTime.Add(config.MaxDelTime);

        // Time remaining = deadline - current time
        TimeSpan timeLeft = maxDeliveryDeadline - config.Clock;

        // Return zero if already past deadline
        return timeLeft > TimeSpan.Zero ? timeLeft : TimeSpan.Zero;
    }

    internal static char AccessToData(int userId, object data)
    {
        var adminId = AdminManager.GetConfig().AdminId;

        // Check if data is a Courier and assign to variable
        if (data is BO.Courier courier)
        {
            return userId == courier.Id ? 'C' : 'N';
        }
        
        // Check if data is an Order and assign to variable
        if (data is BO.Order order)
        {
            // For orders, maybe check if user placed the order?
            return userId == order.Id ? 'O' : 'N';
        }

        // Default case for other data types
        return userId == adminId ? 'A' : 'N'; // Admin has full access
    }
}