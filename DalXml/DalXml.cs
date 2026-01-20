using DalApi;
using System.Diagnostics;
namespace Dal;

/// <summary>
/// class DalXml that implements the IDal interface for managing data in XML format.
/// </summary>
sealed internal class DalXml : IDal
{
    private static readonly Lazy<IDal> s_instance = new(() => new DalXml(), isThreadSafe: true);
    public static IDal Instance => s_instance.Value;
    private DalXml() { }

    // Singleton pattern implementation
    public IConfig Config { get; } = new ConfigImplementation();

    public ICourier Courier =>  new CourierImplementation();

    public IOrder Order =>  new OrderImplementation();

    public IDelivery Delivery =>  new DeliveryImplementation();

    // Resets the entire database by deleting all entries in Courier, Order, and Delivery, and resetting the configuration.
    public void ResetDB()
    {
        Courier.DeleteAll();
        Order.DeleteAll();
        Delivery.DeleteAll();
        Config.Reset();
    }
}

