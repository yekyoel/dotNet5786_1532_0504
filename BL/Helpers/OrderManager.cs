using DalApi;
using DO;
namespace Helpers;

internal static class OrderManager
{
    private static IDal s_dal = Factory.Get;

    /// <summary>
    /// Retrieves a single order by ID as a BO.Order object.
    /// </summary>
    internal static BO.Order? GetOrderById(int orderId)
    {
        var dalOrder = s_dal.Order.Read(orderId);

        if (dalOrder == null)
            return null;

        return new BO.Order
        {
            Id = dalOrder.Id.ToString(),
            OrderTyype = Tools.SwitchOrderTypeTOBO(dalOrder),
            Description = dalOrder.Description,
            OrderAddress = dalOrder.FullAdd,
            Latitude = dalOrder.Latitude,
            Longitude = dalOrder.Longitude,
            AerialDistance = dalOrder.Latitude, //?
            CustomerName = dalOrder.CustFullName,
            CustomerPhone = dalOrder.CusNum,
            Weight = dalOrder.Weight,
            OrderPlacedTime = dalOrder.StartTimeForOrdering ?? DateTime.Now,
            ExpectedDeliveryTime = null, // i need a function to calculate it
            MaxDeliveredTime = DateTime.Now.AddHours(24), // i need a function to calculate it
            OrderStatus = Tools.FindOrderStatusType(dalOrder),
            ScheduleStatus = DeliveryManager.checkForSatusTwo(dalOrder),
            TotalTimeLeft = TimeSpan.Zero, // i need a function to calculate it
            DeliveriesList = new List<BO.DeliveryPerOrderInList>()
        };
    }

    /// <summary>
    /// Retrieves all orders as simplified OrderInList view models.
    /// </summary>
    internal static IEnumerable<BO.OrderInList> GetAllOrders()
    {
        var dalOrders = s_dal.Order.ReadAll();

        return dalOrders.Select.Distinct(dalOrder => new BO.OrderInList
        {
            DeliveryId = null,
            OrderId = dalOrder.Id,
            OrderType = dalOrder.Food ,
            AerialDistance = dalOrder.AerialDistance,
            OrderStatus = DeliveryManager.checkForSatus(dalOrder),
            ScheduleStatus = DeliveryManager.checkForSatusTwo(dalOrder),
            TotalTimeLeft = TimeSpan.Zero, // i need a function to calculate it
            TotalCompletionTime = TimeSpan.Zero, // i need a function to calculate it
            TotalDeliveries = 0 // i need a function to calculate it
        });
    }

    internal static void UpdateOrder(string userID, BO.Order order)
    {
        if(order == null)
            throw  "Order cannot be null";
        var dalOrder = s_dal.Order.Read(int.Parse(order.Id));
    
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

    internal static void TryToDeleteOrder(orderId);

    internal static void AddOrder();


}

