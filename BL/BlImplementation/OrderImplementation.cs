using BlApi;
using BO;
using DalApi;
using DO;
using Helpers;

namespace BlImplementation;

internal class OrderImplementation : IOrder 
{
    //???
    public int[] StatusTotal(string userId)
    {

        throw new NotImplementedException();
    }

    //???
    public IEnumerable<BO.OrderInList> GetListOfOrders(string userId, OrderInListFilter? filter, object? filterTwo, OrderInListFilter? sort)
    {
        IEnumerable<BO.OrderInList> allOrders = Helpers.OrderManager.GetAllOrders();
        
        //if(allOrders.)

        if(filter == null)
            return (allOrders = allOrders.Select(o => o).Distinct()).ToList();
        else if(filterTwo )

        switch (filter)
        {
            case OrderInListFilter.OrderId:
                allOrders = allOrders.Where(o => o.OrderId == (int)nullable!);
                break;
            case OrderInListFilter.OrderType:
                allOrders = allOrders.Where(o => o.OrderType == (OrderType)nullable!);
                break;
            case OrderInListFilter.OrderStatus:
                allOrders = allOrders.Where(o => o.OrderStatus == (OrderStatus)nullable!);
                break;
            case OrderInListFilter.ScheduleStatus:
                allOrders = allOrders.Where(o => o.ScheduleStatus == (ScheduleStatus)nullable!);
                break;
            default:
                break;
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


    public void AddOrder(int userId, BO.Order order)
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




    public void OrderComplete(int userId, int courierId, int deliveryId)
    {
        throw new NotImplementedException();
    }

   

  
}
