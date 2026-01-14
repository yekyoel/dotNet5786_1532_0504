namespace BlApi;

public interface IOrder : IObservable
{
    public int[] StatusTotal(int userId); 

    public IEnumerable<BO.OrderInList> GetListOfOrders(int userId, BO.OrderInListFilter? fliter, object? nullable, BO.OrderInListFilter? sort);

    public BO.Order GetOrderDetails(int userId, int orderId);

    public void UpdateOrderDetails(int userId, BO.Order order);

    public void CancelOrder(int userId, int orderId);

    public void DeleteOrder(int userId, int orderId);

    public Task AddOrder(int userId, BO.Order order);

    public void OrderComplete(int userId , int courierId, int deliveryId);

    public void ChooseOrder(int userId , int courierId, int orderId);

    public IEnumerable<BO.ClosedDeliveryInList> GetCompletedCourierDeliveries(int userId, int courierId, BO.ClosedDeliveryInListFilter? filter, BO.ClosedDeliveryInListFilter? sort);

    public IEnumerable<BO.OpenOrderInList> GetAvailableOrdersForCourier(int userId, int courierId, BO.OpenOrderInListFilter? filter, BO.OpenOrderInListFilter? sort);

    public Task<IEnumerable<BO.OpenOrderInList>> GetAvailableOrdersForCourierAsync(int userId, int courierId, BO.OpenOrderInListFilter? filter, BO.OpenOrderInListFilter? sort);
}