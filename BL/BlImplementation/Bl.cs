namespace BlImplementation;

using BlApi;
internal class Bl : IBl
{
    public ICourier Courier { get; } = new CourierImplementation();

    public IOrder Order { get; } = new OrderImplementation();

    public IAdmin Admin { get; } = new AdminImplementation();

}
