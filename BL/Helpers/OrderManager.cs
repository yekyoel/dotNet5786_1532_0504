using DalApi;
using DO;
using System.Runtime.CompilerServices;

namespace Helpers;

/*internal static class OrderManager
{
    private static IDal s_dal = Factory.Get; //stage 4

    internal static BO.Order GetOrder() => (BO.Order)s_dal.Order;

    internal static IEnumerable<BO.OrderInList> GetAllOrders()
    {
        
        return (IEnumerable<BO.OrderInList>)s_dal.Order.ReadAll();
    }


}*/

internal static class OrderManager
{
    private static IDal s_dal = Factory.Get;

    /// <summary>
    /// Retrieves a single order by ID as a full BO.Order object.
    /// </summary>
    internal static BO.Order? GetOrderById(int orderId)
    {
        var dalOrder = s_dal.Order.Read(orderId);

        if (dalOrder == null)
            return null;

        return new BO.Order
        {
            Id = dalOrder.Id.ToString(),
          //  OrderTyype = dalOrder.OrderType, // check why this doesnt work
            Description = dalOrder.Description,
            OrderAddress = dalOrder.FullAdd,
            Latitude = dalOrder.Latitude,
            Longitude = dalOrder.Longitude,
            //AerialDistance = dalOrder.AerialDistance,
            CustomerName = dalOrder.CustFullName,
            CustomerPhone = dalOrder.CusNum,
            Weight = dalOrder.Weight,
            OrderPlacedTime = dalOrder.StartTimeForOrdering ?? DateTime.Now,
            ExpectedDeliveryTime = null,
            MaxDeliveredTime = DateTime.Now.AddHours(24),
            OrderStatus = BO.OrderStatus.Open,
            ScheduleStatus = BO.ScheduleStatus.OnTime,
            TotalTimeLeft = TimeSpan.Zero,
            DeliveriesList = new List<BO.DeliveryPerOrderInList>()
        };
    }

    /// <summary>
    /// Retrieves all orders as simplified OrderInList view models.
    /// </summary>
    internal static IEnumerable<BO.OrderInList> GetAllOrders()
    {
        var dalOrders = s_dal.Order.ReadAll();

        return dalOrders.Select(dalOrder => new BO.OrderInList
        {
            DeliveryId = null,
            OrderId = dalOrder.Id,
            OrderType = dalOrder.Food ?? BO.OrderType.Pizza,
            AerialDistance = dalOrder.AerialDistance,
            OrderStatus = BO.OrderStatus.Open,
            ScheduleStatus = BO.ScheduleStatus.OnTime,
            TotalTimeLeft = TimeSpan.Zero,
            TotalCompletionTime = TimeSpan.Zero,
            TotalDeliveries = 0
        });
    }

    internal static void UpdateOrder(BO.Order order)
    {
        var dalOrder = s_dal.Order.Read(int.Parse(order.Id));
        if (dalOrder == null)
            throw new KeyNotFoundException($"Order with ID {order.Id} not found");
        s_dal.Order.Update(dalOrder);
    }

    internal static void TryToCancelOrder(int orderId)
    {
        var dalOrder = s_dal.Order.Read(orderId);
        if (dalOrder == null)
            throw new KeyNotFoundException($"Order with ID {orderId} not found");
        else if (DeliveryManager.checkForSatus(dalOrder) == DO.CompletionType.Pending) // its open
        {
            new DO.Order
            {
                Id = 0,
                Latitude = dalOrder.Latitude,
                Longitude = dalOrder.Longitude,
                Weight = dalOrder.Weight,
                FullAdd = dalOrder.FullAdd,
                CustFullName = dalOrder.CustFullName,
                CusNum = dalOrder.CusNum,
                StartTimeForOrdering = dalOrder.StartTimeForOrdering,
                Description = dalOrder.Description,
                Food = dalOrder.Food
                // update other fields as necessary
            };
            s_dal.Order.Create(dalOrder);
        }
        else if (DeliveryManager.checkForSatus(dalOrder) == DO.CompletionType.EnRoute) // being handeled
            s_dal.Delivery.Update(dalOrder);
        // finish
        else
            throw new InvalidOperationException($"Order with ID {orderId} cannot be cancelled as it is already completed or cancelled.");
    }
}

