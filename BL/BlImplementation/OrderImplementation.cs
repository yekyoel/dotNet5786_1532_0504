using BlApi;
using BO;
using Helpers;

namespace BlImplementation;

internal class OrderImplementation : IOrder 
{
    //???
    public int[] StatusTotal(int userId)
    {
        //
    }

    //???
    public IEnumerable<BO.OrderInList> GetListOfOrders(int userId, OrderInListFilter? filter, object? filterTwo, OrderInListFilter? sort)
    {
        IEnumerable<BO.OrderInList> allOrders = Helpers.OrderManager.GetAllOrders();

        allOrders = allOrders.Distinct();
        if (filter == null)
        {

        }
    }

    public BO.Order GetOrderDetails(int userId, int orderId)
    {
        var order = Helpers.OrderManager.GetOrderById(orderId);

        if (order == null)
            throw new KeyNotFoundException($"Order with ID {orderId} not found");

        return order;
    }


    public void UpdateOrderDetails(int userId, BO.Order order)
    {
        Helpers.OrderManager.UpdateOrder(order);
    }

    public void CancelOrder(int userId, int orderId)
    {
        Helpers.OrderManager.TryToCancelOrder(orderId);
    }

    public void DeleteOrder(int userId, int orderId)
    {
        Helpers.OrderManager.TryToDeleteOrder(orderId);
    }

    public void AddOrder(int userId, BO.Order order)
    {
        Helpers.OrderManager.AddOrder(order);
    }


    public void OrderComplete(int userId, int courierId, int deliveryId)
    {
        if(userId == courierId)
        {
            Helpers.DeliveryManager.CompleteDelivery(deliveryId);
        }
        else
        {
            throw new InvalidOperationException("User is not authorized to complete this delivery.");
        }
    }

    public void ChooseOrder(int userId, int courierId, int orderId)
    {
        if (userId == courierId)
        {
            Helpers.OrderManager.AssignOrderToCourier(orderId, courierId);
        }
        else
        {
            throw new InvalidOperationException("User is not authorized to choose this order.");
        }
    }

    public ClosedDeliveryInList GetCompletedCourierDeliveries(int userId, int courierId, ClosedDeliveryInListFilter? filter, ClosedDeliveryInListFilter? sort)
    {
        return 

    }

    public OpenOrderInList GetAvailableOrdersForCourier(int userId, int courierId, OpenOrderInListFilter? filter, OpenOrderInListFilter? sort)
    {
        
    } 
}
