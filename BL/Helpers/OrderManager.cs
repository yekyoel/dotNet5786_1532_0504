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

        var cfg = AdminManager.GetConfig();
        double storeLat = cfg?.Latitude ?? 0.0;
        double storeLon = cfg?.Longitude ?? 0.0;
        double aerial = Tools.GetAerialDistanceKm(storeLat, storeLon, dalOrder.Latitude, dalOrder.Longitude);

        return new BO.Order
        {
            Id = dalOrder.Id.ToString(),
            OrderTyype = Tools.SwitchOrderTypeTOBO(dalOrder),
            Description = dalOrder.Description,
            OrderAddress = dalOrder.FullAdd,
            Latitude = dalOrder.Latitude,
            Longitude = dalOrder.Longitude,
            AerialDistance = aerial,
            CustomerName = dalOrder.CustFullName,
            CustomerPhone = dalOrder.CusNum,
            Weight = dalOrder.Weight,
            OrderPlacedTime = dalOrder.StartTimeForOrdering ?? DateTime.Now,
            ExpectedDeliveryTime = null, // i need a function to calculate it
            MaxDeliveredTime = DateTime.Now.AddHours(24), // i need a function to calculate it
            OrderStatus = Tools.FindOrderStatusType(dalOrder),
            ScheduleStatus = Tools.FindScheduleStatusType(dalOrder),
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
            OrderStatus = Tools.FindOrderStatusType(dalOrder),
            ScheduleStatus = Tools.FindScheduleStatusType(dalOrder),
            TotalTimeLeft = TimeSpan.Zero, // i need a function to calculate it
            TotalCompletionTime = TimeSpan.Zero, // i need a function to calculate it
            TotalDeliveries = 0 // i need a function to calculate it
        });
    }

    internal static void UpdateOrder(string userID, BO.Order order)
    {
        if(order == null)
            throw  new "Order cannot be null";
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
        else if (DeliveryManager.checkForSatus(dalOrder) == DO.CompletionType.Refused) // being handeled
            s_dal.Delivery.Update(dalOrder);
        // finish
        else
            throw new InvalidOperationException($"Order with ID {orderId} cannot be cancelled as it is already completed or cancelled.");
    }

    internal static void TryToDeleteOrder(orderId);

    internal static void AddOrder();


    internal static double GetAerialDistanceKm(double lat1, double lon1, double lat2, double lon2)
{
    const double R = 6371.0;
    static double ToRad(double deg) => deg * Math.PI / 180.0;
    var dLat = ToRad(lat2 - lat1);
    var dLon = ToRad(lon2 - lon1);
    var a = Math.Sin(dLat/2)*Math.Sin(dLat/2)
          + Math.Cos(ToRad(lat1))*Math.Cos(ToRad(lat2))
          * Math.Sin(dLon/2)*Math.Sin(dLon/2);
    var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    return R * c; // kilometers
}

