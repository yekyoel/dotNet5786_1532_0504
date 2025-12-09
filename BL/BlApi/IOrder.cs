namespace BlApi;

public interface IOrder
{
    public int[] StatusTotal(string userId); /// check?

    public IEnumerable<BO.OrderInList> GetListOfOrders(string userId, BO.OrderInListFilter? fliter, object? nullable, BO.OrderInListFilter? sort);

    public BO.Order GetOrderDetails(int userId, int orderId);

    public void UpdateOrderDetails(int userId, BO.Order order);

    public void CancelOrder(int userId, int orderId);

    public void DeleteOrder(int userId, int orderId);

    public void AddOrder(int userId, BO.Order order);

    public void OrderComplete(int userId , int courierId, int deliveryId);

    public void ChooseOrder(int userId , int courierId, int orderId);

    public BO.ClosedDeliveryInList GetCompletedCourierDeliveries(int userId, int courierId, BO.ClosedDeliveryInListFilter? filter, BO.ClosedDeliveryInListFilter? sort);

    public BO.OpenOrderInList GetAvailableOrdersForCourier(int userId, int courierId, BO.OpenOrderInListFilter? filter, BO.OpenOrderInListFilter? sort);

}