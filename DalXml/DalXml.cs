using DalApi;
namespace Dal;

//stage 3
sealed public class DalXml : IDal
{
    // Singleton pattern implementation
    public IConfig Config { get; } = new ConfigImplementation();

    public ICourier Courier =>  new CourierImplementation();

    public IOrder Order =>  new OrderImplementation();

    public IDelivery Delivery =>  new DeliveryImplementation();

    public void ResetDB()
    {
        Courier.DeleteAll();
        Order.DeleteAll();
        Delivery.DeleteAll();
        Config.Reset();
    }
}

