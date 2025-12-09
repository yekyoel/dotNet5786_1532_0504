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
}

