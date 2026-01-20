namespace Dal;
using DalApi;

/// <summary>
/// Provides a data access layer implementation using in-memory lists for couriers, orders, deliveries, and configuration.
/// </summary>

sealed internal class DalList : IDal
{
    private static readonly Lazy<IDal> s_instance = new(() => new DalList(), isThreadSafe: true);
    public static IDal Instance => s_instance.Value;
    private DalList() { }

    public ICourier Courier { get; } =  new CourierImplementation();

    public IOrder Order { get; } = new OrderImplementation();

    public IDelivery Delivery { get; } = new DeliveryImplementation();

    public IConfig Config { get; } = new ConfigImplementation();

    /// <summary>
    /// Resets the in-memory database by clearing all couriers, orders, deliveries, and resetting the configuration.
    /// </summary>
    public void ResetDB()
    {
        Courier.DeleteAll();
        Order.DeleteAll();
        Delivery.DeleteAll();
        Config.Reset();
    }
}
