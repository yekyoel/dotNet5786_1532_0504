using BO;
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
            throw new BLNullReferenceException ("Order cannot be null");
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

    internal static void TryToDeleteOrder(int orderId)
    {
        var dalOrder = s_dal.Order.Read(orderId);
        if (dalOrder == null)
            throw new KeyNotFoundException($"Order with ID {orderId} not found");
        else
        {
            s_dal.Order.Delete(orderId);
        }
    }

    // Parameterless helper left for compatibility; instruct callers to use overload below.
    internal static void AddOrder()
    {
        throw new NotImplementedException("Call AddOrder(BO.Order order) overload with a BO.Order argument.");
    }

    // Adds a new BO.Order into the DAL by mapping it to DO.Order and calling DAL.Create.
    internal static void AddOrder(BO.Order order)
    {
        if (order is null)
            throw new BO.BLInvalidOrderException("Order cannot be null.");

        // Basic validation
        if (string.IsNullOrWhiteSpace(order.OrderAddress)
            || string.IsNullOrWhiteSpace(order.CustomerName)
            || string.IsNullOrWhiteSpace(order.CustomerPhone))
        {
            throw new BO.BLInvalidOrderException("Order missing required customer or address information.");
        }

        if (order.Weight <= 0)
            throw new BO.BLInvalidOrderException("Order weight must be greater than zero.");

        if (order.Latitude < -90.0 || order.Latitude > 90.0
            || order.Longitude < -180.0 || order.Longitude > 180.0)
        {
            throw new BO.BLInvalidOrderException("Order latitude/longitude out of range.");
        }

        // use DAL clock if BO.Order.OrderPlacedTime is default/unset
        var configClock = AdminManager.GetConfig()?.Clock;
        DateTime? placed = order.OrderPlacedTime == default(DateTime)
                            ? configClock ?? DateTime.Now
                            : order.OrderPlacedTime;

        // Map BO.Order -> DO.Order
        var doOrder = new DO.Order
        (
            Id: 0,
            Latitude: order.Latitude,
            Longitude: order.Longitude,
            Weight: order.Weight,
            FullAdd: order.OrderAddress ?? string.Empty,
            CustFullName: order.CustomerName ?? string.Empty,
            CusNum: order.CustomerPhone ?? string.Empty,
            StartTimeForOrdering: placed,
            Description: order.Description,
            Food: Tools.SwitchOrderTypeTODO(order) // returns DO.OrderType? 
        );

        s_dal.Order.Create(doOrder);
    }


    /// <summary>
    /// Periodic updates specific to orders/deliveries. Called after clock changes.
    /// Policy (non-destructive / conservative):
    /// - If a delivery is in-progress (has ShippingMethod but no End/DeliveryEndTime)
    ///   and has exceeded the configured maximum delivery time plus risk window,
    ///   mark the delivery as Failed and set DeliveryEndTime to the new clock.
    /// - Method is lightweight and resilient: exceptions are swallowed so clock update will not fail.
    /// </summary>
    internal static void PeriodicOrdersUpdates(DateTime oldClock, DateTime newClock)
    {
        try
        {
            if (newClock <= oldClock)
                return;

            var config = AdminManager.GetConfig();
            if (config == null)
                return;

            // Read once
            var deliveries = s_dal.Delivery.ReadAll();

            foreach (var d in deliveries)
            {
                try
                {
                    // skip already finished deliveries
                    if (d.DeliveryEndTime.HasValue)
                        continue;

                    // must be assigned (in-progress) to consider as overdue
                    if (d.ShippingMethod is null)
                        continue;

                    // determine start reference: delivery start, or order ordering time, or config clock
                    var order = s_dal.Order.Read(d.OrderId);
                    DateTime referenceStart = d.DeliveryStartTime
                                              ?? order?.StartTimeForOrdering
                                              ?? config.Clock;

                    DateTime expectedDeliveryTime = referenceStart.Add(config.MaxDelTime);
                    DateTime failThreshold = expectedDeliveryTime.Add(config.RiskRange);

                    // if we're past the fail threshold, mark as failed
                    if (newClock > failThreshold)
                    {
                        var updated = d with
                        {
                            End = DO.CompletionType.Failed,
                            DeliveryEndTime = newClock
                        };
                        s_dal.Delivery.Update(updated);
                    }
                }
                catch
                {
                    // swallow per-delivery update failure and continue
                }
            }
        }
        catch
        {
            // swallow outer exceptions to avoid breaking clock update caller
        }
    }

    /// <summary>
    /// Conservative auto-assignment of pending/open orders to available couriers.
    /// Policy:
    /// - Only assigns when there is exactly one clearly eligible and currently free courier.
    /// - Creates a DO.Delivery with End=Pending and DeliveryStartTime set to the new clock.
    /// - Lightweight and resilient: exceptions are swallowed per-order.
    /// </summary>
    internal static void PeriodicAutoAssignPendingOrders(DateTime oldClock, DateTime newClock)
    {
        try
        {
            if (newClock <= oldClock)
                return;

            var config = AdminManager.GetConfig();
            if (config == null)
                return;

            var orders = s_dal.Order.ReadAll().ToList();
            var couriers = s_dal.Courier.ReadAll().ToList();
            var deliveries = s_dal.Delivery.ReadAll().ToList();

            foreach (var order in orders)
            {
                try
                {
                    // skip orders that already have a delivery
                    if (DeliveryManager.GetDeliveryByOrderId(order.Id) != null)
                        continue;

                    // only consider truly open orders
                    if (Tools.FindOrderStatusType(order) != BO.OrderStatus.Open)
                        continue;

                    // distance from store to order
                    double storeLat = config.Latitude ?? 0.0;
                    double storeLon = config.Longitude ?? 0.0;
                    double distanceKm = GetAerialDistanceKm(storeLat, storeLon, order.Latitude, order.Longitude);

                    // eligible couriers: active, within max distance, and currently free (no in-progress deliveries)
                    var eligible = couriers.Where(c =>
                        c.IsActive
                        && (c.MaxDist == null || c.MaxDist >= distanceKm)
                        && !deliveries.Any(d => d.CourierId == c.Id && !d.DeliveryEndTime.HasValue)
                    ).ToList();

                    // conservative assignment: only if exactly one clear candidate
                    if (eligible.Count != 1)
                        continue;

                    var chosen = eligible[0];
                    var method = chosen.PreferredShippingMethod ?? DO.ShippingMethod.Car;

                    var newDelivery = new DO.Delivery
                    {
                        Id = 0,
                        OrderId = order.Id,
                        CourierId = chosen.Id,
                        ShippingMethod = method,
                        DeliveryStartTime = newClock,
                        Distance = distanceKm,
                        End = DO.CompletionType.Pending,
                        DeliveryEndTime = null
                    };

                    s_dal.Delivery.Create(newDelivery);
                }
                catch
                {
                    // swallow per-order failures and continue
                }
            }
        }
        catch
        {
            // swallow outer exceptions
        }
    }

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

}

