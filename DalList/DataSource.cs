namespace Dal;
/// <summary>
/// Data Source class containing in-memory lists for Couriers, Deliveries, and Orders.
/// </summary>
internal static class DataSource
{
    internal static List<DO.Courier> Couriers { get; } = new(); 
    internal static List<DO.Delivery> Deliveries { get; } = new();
    internal static List<DO.Order> Orders { get; } = new();



}
