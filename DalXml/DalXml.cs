using DalApi;
namespace Dal;

/// <summary>
/// class DalXml that implements the IDal interface for managing data in XML format.
/// </summary>
sealed public class DalXml : IDal
{
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

