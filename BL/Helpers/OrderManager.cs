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

        var delivery = DeliveryManager.GetDeliveryByOrderId(orderId);
        var expectedDeliveryTime = Tools.CalculateExpectedDeliveryTime(dalOrder, delivery);
        var maxDeliveredTime = (dalOrder.StartTimeForOrdering ?? DateTime.Now).Add(cfg?.MaxDelTime ?? TimeSpan.FromHours(24));
        var totalTimeLeft = Tools.CalculateTotalTimeLeft(dalOrder, delivery);

        return new BO.Order
        {
            Id = dalOrder.Id,
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
            ExpectedDeliveryTime = expectedDeliveryTime,
            MaxDeliveredTime = maxDeliveredTime,
            OrderStatus = Tools.FindOrderStatusType(dalOrder),
            ScheduleStatus = Tools.FindScheduleStatusType(dalOrder),
            TotalTimeLeft = totalTimeLeft,
            DeliveriesList = new List<BO.DeliveryPerOrderInList>()
        };
    }

    /// <summary>
    /// Retrieves all orders as simplified OrderInList view models.
    /// </summary>
    internal static IEnumerable<BO.OrderInList> GetAllOrders()
    {
        var dalOrders = s_dal.Order.ReadAll();

        var cfg = AdminManager.GetConfig();
        double storeLat = cfg?.Latitude ?? 0.0;
        double storeLon = cfg?.Longitude ?? 0.0;

        return dalOrders.Select(dalOrder =>
        {
            var delivery = DeliveryManager.GetDeliveryByOrderId(dalOrder.Id);
            var totalTimeLeft = Tools.CalculateTotalTimeLeft(dalOrder, delivery);
            var completionTime = (delivery?.DeliveryEndTime.HasValue ?? false) && (delivery?.DeliveryStartTime.HasValue ?? false)
                ? delivery.DeliveryEndTime.Value - delivery.DeliveryStartTime.Value
                : TimeSpan.Zero;

            return new BO.OrderInList
            {
                DeliveryId = delivery?.Id,
                OrderId = dalOrder.Id,
                OrderType = Tools.SwitchOrderTypeTOBO(dalOrder) ?? BO.OrderType.Pizza,
                AerialDistance = Tools.GetAerialDistanceKm(storeLat, storeLon, dalOrder.Latitude, dalOrder.Longitude),
                OrderStatus = Tools.FindOrderStatusType(dalOrder) ?? BO.OrderStatus.Open,
                ScheduleStatus = Tools.FindScheduleStatusType(dalOrder) ?? BO.ScheduleStatus.OnTime,
                TotalTimeLeft = totalTimeLeft,
                TotalCompletionTime = completionTime,
                TotalDeliveries = delivery != null ? 1 : 0
            };
        }).ToList();
    }

   
    internal static void UpdateOrder(BO.Order order)
    {
        if (order == null)
            throw new BLNullReferenceException("Order cannot be null");
        var dalOrder = s_dal.Order.Read(order.Id);

        s_dal.Order.Update(dalOrder);
    }

    internal static void TryToCancelOrder(int orderId)
    {
        Delivery? delivery;
        var dalOrder = s_dal.Order.Read(orderId);
        if (dalOrder == null)
            throw new KeyNotFoundException($"Order with ID {orderId} not found");
        else if (Tools.FindOrderStatusType(dalOrder) == BO.OrderStatus.Open) // its open
        {
            var del = new DO.Delivery
            {
                OrderId = dalOrder.Id,
                CourierId = 0,
                DeliveryStartTime = AdminManager.GetConfig()?.Clock ?? DateTime.Now,
                DeliveryEndTime = AdminManager.GetConfig()?.Clock ?? DateTime.Now,
                End = DO.CompletionType.Cancelled,
            };
            s_dal.Delivery.Create(del);
        }
        else if (Tools.FindOrderStatusType(dalOrder) == BO.OrderStatus.InProgress)  // being handeled
        {
            delivery = DeliveryManager.GetDeliveryByOrderId(orderId);
            if (delivery == null)
                throw new InvalidOperationException($"Order with ID {orderId} is in progress but has no associated delivery.");
            var dalDelivery = new DO.Delivery
            {
                Id = delivery.Id,
                OrderId = delivery.OrderId,
                CourierId = delivery.CourierId,
                ShippingMethod = delivery.ShippingMethod,
                DeliveryStartTime = delivery.DeliveryStartTime,
                Distance = delivery.Distance,
                End = DO.CompletionType.Cancelled,
                DeliveryEndTime = AdminManager.GetConfig()?.Clock ?? DateTime.Now
            };
            s_dal.Delivery.Update(dalDelivery);
        }
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

        var doOrder = new DO.Order
        {
            Id = order.Id,
            Latitude = order.Latitude,
            Longitude = order.Longitude,
            Weight = order.Weight,
            FullAdd = order.OrderAddress ?? string.Empty,
            CustFullName = order.CustomerName ?? string.Empty,
            CusNum = order.CustomerPhone ?? string.Empty,
            StartTimeForOrdering = placed,
            Description = order.Description,
            Food = Tools.SwitchOrderTypeTODO(order) // returns DO.OrderType? 
        };

        s_dal.Order.Create(doOrder);
    }

    internal static void AssignOrderToCourier(int orderId, int courierId)
    {
        var delivery = DeliveryManager.GetDeliveryByOrderId(orderId);
        if (delivery != null && delivery.End == DO.CompletionType.Pending)
        {
            var updatedDelivery = delivery with
            {
                CourierId = courierId,
                DeliveryStartTime = AdminManager.GetConfig()?.Clock ?? DateTime.Now,
                DeliveryEndTime = null,
                ShippingMethod = null
            };
            s_dal.Delivery.Create(updatedDelivery);

        }
        else
            throw new InvalidOperationException($"Order with ID {orderId} is not pending assignment or already has a delivery.");

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
                    double distanceKm = Tools.GetAerialDistanceKm(storeLat, storeLon, order.Latitude, order.Longitude);

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


    internal static IEnumerable<BO.ClosedDeliveryInList> GetClosedDeliveries(int courierId, ClosedDeliveryInListFilter? filter, ClosedDeliveryInListFilter? sort)
    {
        var dalDeliveries = DeliveryManager.GetAllDeliveries()
            .Where(d => d.CourierId == courierId && d.End == DO.CompletionType.Delivered).ToList();

        // Apply filtering if specified
        if (filter.HasValue)
        {
            dalDeliveries = ApplyClosedDeliveryFilter(dalDeliveries, filter.Value);
        }

        // Apply sorting if specified
        if (sort.HasValue)
        {
            dalDeliveries = ApplyClosedDeliverySort(dalDeliveries, sort.Value);
        }

        // Map to BO
        return dalDeliveries.Select(dalDelivery =>
        {
            var order = s_dal.Order.Read(dalDelivery.OrderId);
            return new BO.ClosedDeliveryInList
            {
                DeliveryId = dalDelivery.Id,
                OrderId = dalDelivery.OrderId,
                OrderType = Tools.SwitchOrderTypeTOBO(order) ?? BO.OrderType.Pizza,
                DeliveryAddress = order?.FullAdd ?? string.Empty,
                DeliveryType = Tools.SwitchShippingMethodTOBO(dalDelivery.ShippingMethod) ?? BO.ShippingMethod.Car,
                ActualDistance = dalDelivery.Distance ?? 0,
                TotalCompletionTime = (dalDelivery.DeliveryEndTime.HasValue && dalDelivery.DeliveryStartTime.HasValue)
                    ? dalDelivery.DeliveryEndTime.Value - dalDelivery.DeliveryStartTime.Value
                    : TimeSpan.Zero,
                CompletionType = Tools.SwitchCompletionTypeTOBO(dalDelivery.End) ?? BO.CompletionType.Delivered
            };
        }).ToList();
    }

    private static List<DO.Delivery> ApplyClosedDeliveryFilter(List<DO.Delivery> deliveries, ClosedDeliveryInListFilter filter)
    {
        return filter switch
        {
            ClosedDeliveryInListFilter.DeliveryId => deliveries,
            ClosedDeliveryInListFilter.OrderId => deliveries,
            ClosedDeliveryInListFilter.OrderType => deliveries,
            ClosedDeliveryInListFilter.DeliveryAddress => deliveries,
            ClosedDeliveryInListFilter.DeliveryType => deliveries,
            ClosedDeliveryInListFilter.ActualDistance => deliveries,
            ClosedDeliveryInListFilter.TotalCompletionTime => deliveries,
            ClosedDeliveryInListFilter.CompletionType => deliveries,
            _ => deliveries
        };
    }

    private static List<DO.Delivery> ApplyClosedDeliverySort(List<DO.Delivery> deliveries, ClosedDeliveryInListFilter sort)
    {
        return sort switch
        {
            ClosedDeliveryInListFilter.DeliveryId => deliveries.OrderBy(d => d.Id).ToList(),
            ClosedDeliveryInListFilter.OrderId => deliveries.OrderBy(d => d.OrderId).ToList(),
            ClosedDeliveryInListFilter.ActualDistance => deliveries.OrderBy(d => d.Distance ?? 0).ToList(),
            ClosedDeliveryInListFilter.TotalCompletionTime => deliveries.OrderBy(d =>
                (d.DeliveryEndTime ?? DateTime.Now) - (d.DeliveryStartTime ?? DateTime.Now)).ToList(),
            _ => deliveries
        };
    }


    internal static IEnumerable<BO.OpenOrderInList> GetOpenOrders(int courierId, OpenOrderInListFilter? filter, OpenOrderInListFilter? sort)
    {
        var dalOrders = s_dal.Order.ReadAll().ToList();

        // Get courier's max distance capability
        var courier = s_dal.Courier.Read(courierId);
        double courierMaxDist = courier?.MaxDist ?? double.MaxValue;

        var cfg = AdminManager.GetConfig();
        double storeLat = cfg?.Latitude ?? 0.0;
        double storeLon = cfg?.Longitude ?? 0.0;

        // Filter for open orders only (orders without a delivery, or with pending delivery)
        // AND that are within the courier's max distance from the store
        var openOrders = dalOrders.Where(order =>
        {
            var delivery = DeliveryManager.GetDeliveryByOrderId(order.Id);
            bool isOpen = delivery == null || delivery.End == null;
            
            if (!isOpen)
                return false;

            // Check if order is within courier's max distance
            double distance = Tools.GetAerialDistanceKm(storeLat, storeLon, order.Latitude, order.Longitude);
            return distance <= courierMaxDist;
        }).ToList();

        // Apply filtering if specified
        if (filter.HasValue)
        {
            openOrders = ApplyOpenOrderFilter(openOrders, filter.Value);
        }

        // Apply sorting if specified
        if (sort.HasValue)
        {
            openOrders = ApplyOpenOrderSort(openOrders, sort.Value);
        }

        return openOrders.Select(order =>
        {
            var delivery = DeliveryManager.GetDeliveryByOrderId(order.Id);
            double distance = Tools.GetAerialDistanceKm(storeLat, storeLon, order.Latitude, order.Longitude);
            var orderPlacedTime = order.StartTimeForOrdering ?? DateTime.Now;
            var maxDelTime = cfg?.MaxDelTime ?? TimeSpan.FromHours(24);
            var maxDeliveryTime = orderPlacedTime.Add(maxDelTime);
            var expectedDeliveryTime = Tools.CalculateExpectedDeliveryTime(order, delivery);
            var timeSpanToDelivery = expectedDeliveryTime != null 
                ? (expectedDeliveryTime.Value - DateTime.Now)
                : (TimeSpan?)null;
            var totalTimeLeft = Tools.CalculateTotalTimeLeft(order, delivery);

            return new BO.OpenOrderInList
            {
                CourierId = delivery?.CourierId,
                OrderId = order.Id,
                TypeOrder = Tools.SwitchOrderTypeTOBO(order) ?? BO.OrderType.Pizza,
                Weight = order.Weight,
                DeliveryAddress = order.FullAdd ?? string.Empty,
                ArealDistance = distance,
                ActualDistance = delivery?.Distance ?? 0,
                ExpectedActualDeliveryTime = timeSpanToDelivery,
                Status = Tools.FindScheduleStatusType(order) ?? BO.ScheduleStatus.OnTime,
                TotalTimeLeft = totalTimeLeft,
                MaxDeliveryTime = maxDeliveryTime
            };
        }).ToList();
    }

    private static List<DO.Order> ApplyOpenOrderFilter(List<DO.Order> orders, OpenOrderInListFilter filter)
    {
        return filter switch
        {
            OpenOrderInListFilter.CourierId => orders,
            OpenOrderInListFilter.OrderId => orders,
            OpenOrderInListFilter.TypeOrder => orders,
            OpenOrderInListFilter.Weight => orders,
            OpenOrderInListFilter.DeliveryAddress => orders,
            OpenOrderInListFilter.ArealDistance => orders,
            OpenOrderInListFilter.ActualDistance => orders,
            OpenOrderInListFilter.ExpectedActualDeliveryTime => orders,
            OpenOrderInListFilter.Status => orders,
            OpenOrderInListFilter.TotalTimeLeft => orders,
            OpenOrderInListFilter.MaxDeliveryTime => orders,
            _ => orders
        };
    }

    private static List<DO.Order> ApplyOpenOrderSort(List<DO.Order> orders, OpenOrderInListFilter sort)
    {
        var cfg = AdminManager.GetConfig();
        double storeLat = cfg?.Latitude ?? 0.0;
        double storeLon = cfg?.Longitude ?? 0.0;

        return sort switch
        {
            OpenOrderInListFilter.OrderId => orders.OrderBy(o => o.Id).ToList(),
            OpenOrderInListFilter.Weight => orders.OrderBy(o => o.Weight).ToList(),
            OpenOrderInListFilter.ArealDistance => orders.OrderBy(o =>
                Tools.GetAerialDistanceKm(storeLat, storeLon, o.Latitude, o.Longitude)).ToList(),
            OpenOrderInListFilter.ExpectedActualDeliveryTime => orders.OrderBy(o =>
            {
                var delivery = DeliveryManager.GetDeliveryByOrderId(o.Id);
                return delivery?.DeliveryStartTime ?? DateTime.MaxValue;
            }).ToList(),
            OpenOrderInListFilter.MaxDeliveryTime => orders.OrderBy(o => o.StartTimeForOrdering).ToList(),
            _ => orders
        };
    }

}