using BlApi;
using BO;
using Helpers;

namespace BlImplementation;

internal class OrderImplementation : IOrder 
{
    public int[] StatusTotal(int userId)
    {
        var orders = Helpers.OrderManager.GetAllOrders();

        int orderStatusCount =
            Enum.GetValues(typeof(BO.OrderStatus)).Length;

        int scheduleStatusCount =
            Enum.GetValues(typeof(BO.ScheduleStatus)).Length;

        int[] result = new int[orderStatusCount * scheduleStatusCount];

        var grouped = orders.GroupBy(o =>
            (OrderStatus: o.OrderStatus, ScheduleStatus: o.ScheduleStatus));

        foreach (var group in grouped)
        {
            int index =
                (int)group.Key.OrderStatus * scheduleStatusCount
              + (int)group.Key.ScheduleStatus;

            result[index] = group.Count();
        }

        return result;
    }


    private static Func<BO.OrderInList, object?> GetOrderPropertySelector(BO.OrderInListFilter filter)
    {
        return filter switch
        {
            BO.OrderInListFilter.OrderId => o => o.OrderId,
            BO.OrderInListFilter.DeliveryId => o => o.DeliveryId,
            BO.OrderInListFilter.OrderStatus => o => o.OrderStatus,
            BO.OrderInListFilter.OrderType => o => o.OrderType,
            BO.OrderInListFilter.ScheduleStatus => o => o.ScheduleStatus,
            BO.OrderInListFilter.AerialDistance => o => o.AerialDistance,

            _ => _ => null
        };
    }

    public IEnumerable<BO.OrderInList> GetListOfOrders(int userId, BO.OrderInListFilter? filter, object? filterValue, BO.OrderInListFilter? sort)
    {
        // 1️⃣ שליפת כל ההזמנות (כבר עם משלוח אחרון)
        IEnumerable<BO.OrderInList> orders =
            Helpers.OrderManager.GetAllOrders();

        // 2️⃣ הבטחה שכל הזמנה מופיעה פעם אחת
        orders = orders
            .GroupBy(o => o.OrderId)
            .Select(g => g
                .OrderByDescending(o => o.DeliveryId) // TODO: adjust if needed
                .First());

        // 3️⃣ סינון (רק אם נבחר פילטר)
        if (filter != null && filterValue != null)
        {
            var filterSelector = GetOrderPropertySelector(filter.Value);

            orders = orders.Where(o =>
            {
                var value = filterSelector(o);
                return value != null && value.Equals(filterValue);
            });
        }

        // 4️⃣ מיון
        if (sort == null)
        {
            orders = orders.OrderBy(o => o.OrderStatus);
        }
        else
        {
            var sortSelector = GetOrderPropertySelector(sort.Value);
            orders = orders.OrderBy(o => sortSelector(o));
        }

        return orders;
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

    public IEnumerable<BO.ClosedDeliveryInList> GetCompletedCourierDeliveries(int userId, int courierId, BO.ClosedDeliveryInListFilter? filter, BO.ClosedDeliveryInListFilter? sort)
    {
        return Helpers.OrderManager.GetClosedDeliveries(courierId, filter, sort);
    }

    public IEnumerable<BO.OpenOrderInList> GetAvailableOrdersForCourier(int userId, int courierId, BO.OpenOrderInListFilter? filter, BO.OpenOrderInListFilter? sort)
    {
        return Helpers.OrderManager.GetOpenOrders(courierId, filter, sort);
    }
}
