using BlApi;
using BO;
using Helpers;

namespace BlImplementation;

internal class OrderImplementation : IOrder 
{
    //???
    public int[] StatusTotal(string userId)
    {

        throw new NotImplementedException();
    }

    public IEnumerable<OrderInList> GetListOfOrders(string userId, OrderInListFilter? fliter, object? nullable, OrderInListFilter? sort)
    {

        var allOrders = new OrderManager()
            .Where(d => )
            .FirstOfDefault();
    }

    public void AddOrder(int userId, Order order)
    {
        throw new NotImplementedException();
    }

    public void CancelOrder(int userId, int orderId)
    {
        throw new NotImplementedException();
    }

    public void ChooseOrder(int userId, int courierId, int orderId)
    {
        throw new NotImplementedException();
    }

    public void DeleteOrder(int userId, int orderId)
    {
        throw new NotImplementedException();
    }

    public OpenOrderInList GetAvailableOrdersForCourier(int userId, int courierId, OpenOrderInListFilter? filter, OpenOrderInListFilter? sort)
    {
        throw new NotImplementedException();
    }

    public ClosedDeliveryInList GetCompletedCourierDeliveries(int userId, int courierId, ClosedDeliveryInListFilter? filter, ClosedDeliveryInListFilter? sort)
    {
        throw new NotImplementedException();
    }


    public Order GetOrderDetails(int userId, int orderId)
    {
        throw new NotImplementedException();
    }

    public void OrderComplete(int userId, int courierId, int deliveryId)
    {
        throw new NotImplementedException();
    }

   

    public void UpdateOrderDetails(int userId, Order order)
    {
        throw new NotImplementedException();
    }
}
