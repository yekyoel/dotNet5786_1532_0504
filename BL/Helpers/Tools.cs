namespace Helpers;

internal static class Tools
{
    //public static string ToStringProperty<T>(this T t) { }

   internal static bool checkProperty<T>(T t) => t is not null; // checks if property is not null

    internal static object? GetProperty(object obj, string propertyName)
    {
        return obj.GetType().GetProperty(propertyName)?
                  .GetValue(obj, null);
    }

    // Convert DO.Courier.PreferredShippingMethod to BO.ShippingMethod
    internal static BO.ShippingMethod? FindType(DO.Courier courier)
    {
        if (courier.PreferredShippingMethod == DO.ShippingMethod.Car)
            return BO.ShippingMethod.Car;
        else if (courier.PreferredShippingMethod == DO.ShippingMethod.Motorcycle)
            return BO.ShippingMethod.Motorcycle;
        else if (courier.PreferredShippingMethod == DO.ShippingMethod.Bike)
            return BO.ShippingMethod.Bike;
        else if (courier.PreferredShippingMethod == DO.ShippingMethod.OnFoot)
            return BO.ShippingMethod.OnFoot;
        else
            return null;
    }
}

