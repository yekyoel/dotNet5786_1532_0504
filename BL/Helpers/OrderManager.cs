using BO;
using DalApi;
using DO;
namespace Helpers;

internal static class OrderManager
{
    private static IDal s_dal = Factory.Get;
    internal static ObserverManager Observers = new(); // stage 5

    /// <summary>
    /// Retrieves the business object representation of an order by its unique identifier.
    /// </summary>
    /// <remarks>The returned order includes calculated fields such as aerial distance from the store, expected
    /// delivery time, and total time left for delivery. If the order does not exist, the method returns <see
    /// langword="null"/>.</remarks>
    /// <param name="orderId">The unique identifier of the order to retrieve.</param>
    /// <returns>A <see cref="BO.Order"/> object containing the order details if found; otherwise, <see langword="null"/>.</returns>
    internal static BO.Order? GetOrderById(int orderId)
    {
        var dalOrder = s_dal.Order.Read(orderId); // read from DAL

        if (dalOrder == null)
            return null;

        // claculates the soroe cordinates
        var cfg = AdminManager.GetConfig();
        double storeLat = cfg?.Latitude ?? 0.0;
        double storeLon = cfg?.Longitude ?? 0.0;
        double aerial = Tools.GetAerialDistanceKm(storeLat, storeLon, dalOrder.Latitude, dalOrder.Longitude);

        // gets the delivery related to the order
        var delivery = DeliveryManager.GetDeliveryByOrderId(orderId);
        var expectedDeliveryTime = Tools.CalculateExpectedDeliveryTime(dalOrder, delivery);
        var maxDeliveredTime = (dalOrder.StartTimeForOrdering ?? DateTime.Now).Add(cfg?.MaxDelTime ?? TimeSpan.FromHours(24));
        var totalTimeLeft = Tools.CalculateTotalTimeLeft(dalOrder, delivery);

        return new BO.Order // maps DO.Order to BO.Order
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
  /// Retrieves a collection of all orders with summary information for each order.
  /// </summary>
  /// <remarks>Each returned <see cref="BO.OrderInList"/> includes calculated fields such as aerial distance
  /// from the store, order and schedule status, total time left, and delivery completion time. The method aggregates
  /// data from multiple sources to provide a comprehensive overview of all orders.</remarks>
  /// <returns>An enumerable collection of <see cref="BO.OrderInList"/> objects, each representing an order and its associated
  /// summary details. The collection is empty if no orders exist.</returns>
    internal static IEnumerable<BO.OrderInList> GetAllOrders()
    {
        var dalOrders = s_dal.Order.ReadAll(); // read all orders from DAL

        // get store coordinates
        var cfg = AdminManager.GetConfig();
        double storeLat = cfg?.Latitude ?? 0.0;
        double storeLon = cfg?.Longitude ?? 0.0;

        // map each DO.Order to BO.OrderInList with calculated fields
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

   /// <summary>
   /// Updates the specified order in the data store.
   /// </summary>
   /// <param name="order">The order to update. Cannot be null.</param>
   /// <exception cref="BlNullPropertyException">Thrown if <paramref name="order"/> is null.</exception>
    internal static void UpdateOrder(BO.Order order)
    {
        if (order == null)
            throw new BlNullPropertyException("Order cannot be null");
        var dalOrder = s_dal.Order.Read(order.Id);

        s_dal.Order.Update(dalOrder);
    }

    /// <summary>
    /// Attempts to cancel the specified order if it is in an open or in-progress state.
    /// </summary>
    /// <remarks>This method updates the order's status to cancelled if it is eligible for cancellation.
    /// Orders that are already completed or previously cancelled cannot be cancelled again.</remarks>
    /// <param name="orderId">The unique identifier of the order to cancel.</param>
    /// <exception cref="KeyNotFoundException">Thrown if an order with the specified orderId does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the order is in progress but has no associated delivery, or if the order cannot be cancelled because
    /// it is already completed or cancelled.</exception>
    internal static void TryToCancelOrder(int orderId)
    {
        Delivery? delivery;
        var dalOrder = s_dal.Order.Read(orderId); // read order from DAL
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
        else  // completed or cancelled
            throw new InvalidOperationException($"Order with ID {orderId} cannot be cancelled as it is already completed or cancelled.");
    }

    /// <summary>
    /// Attempts to delete the order with the specified identifier. Throws an exception if the order does not exist.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to delete.</param>
    /// <exception cref="KeyNotFoundException">Thrown if an order with the specified orderId does not exist.</exception>
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

    /// <summary>
    /// Adds a new order to the system after validating the provided order details.
    /// </summary>
    /// <param name="order">The order to add. Must include valid customer name, phone number, address, weight greater than zero, and
    /// latitude/longitude within valid ranges.</param>
    /// <exception cref="BO.BLInvalidOrderException">Thrown if the order is null or contains invalid or incomplete information, such as missing customer details,
    /// address, non-positive weight, or out-of-range latitude/longitude.</exception>
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

    /// <summary>
    /// Assigns a pending order to the specified courier for delivery.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to assign. The order must be in a pending state and not already assigned to a
    /// courier.</param>
    /// <param name="courierId">The unique identifier of the courier to whom the order will be assigned.</param>
    /// <exception cref="InvalidOperationException">Thrown if the order is not pending assignment or already has a delivery.</exception>
    internal static void AssignOrderToCourier(int orderId, int courierId)
    { 
        var delivery = DeliveryManager.GetDeliveryByOrderId(orderId); // read delivery from DAL
        if (delivery != null && delivery.End == DO.CompletionType.Pending) 
        {
            var updatedDelivery = delivery with // create a new delivery with updated courier and start time
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

    /// <summary>
    /// Retrieves a collection of closed deliveries for the specified courier, with optional filtering and sorting.
    /// </summary>
    /// <param name="courierId">The unique identifier of the courier whose closed deliveries are to be retrieved.</param>
    /// <param name="filter">An optional filter to apply to the closed deliveries. If null, no filtering is applied.</param>
    /// <param name="sort">An optional sort order to apply to the closed deliveries. If null, the default order is used.</param>
    /// <returns>An enumerable collection of closed deliveries matching the specified criteria. The collection is empty if no
    /// closed deliveries are found for the courier.</returns>
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

    /// <summary>
    /// Returns the list of deliveries after applying the specified closed delivery filter.
    /// </summary>
    /// <param name="deliveries">The list of deliveries to filter. Cannot be null.</param>
    /// <param name="filter">The filter to apply when selecting closed deliveries.</param>
    /// <returns>A list of deliveries that match the specified filter. If no deliveries match, the returned list may be empty.</returns>
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

    /// <summary>
    /// Sorts a list of closed deliveries according to the specified filter.
    /// </summary>
    /// <remarks>The method does not modify the input list; it returns a new sorted list. Sorting by distance
    /// treats null values as zero. Sorting by total completion time uses the current time for any missing start or end
    /// times.</remarks>
    /// <param name="deliveries">The list of closed deliveries to sort. Cannot be null.</param>
    /// <param name="sort">The sorting criterion to apply to the deliveries.</param>
    /// <returns>A new list of deliveries sorted according to the specified filter. If the filter is not recognized, the original
    /// order is preserved.</returns>
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

    /// <summary>
    /// Retrieves a collection of open orders that are available for assignment to the specified courier, optionally
    /// filtered and sorted according to the provided criteria.
    /// </summary>
    /// <remarks>An order is considered open if it has no delivery assigned or its delivery is still pending.
    /// Only orders within the courier's maximum delivery distance from the store are included. Filtering and sorting
    /// are applied only if the corresponding parameters are specified.</remarks>
    /// <param name="courierId">The unique identifier of the courier for whom to retrieve open orders. Determines the maximum delivery distance
    /// for eligible orders.</param>
    /// <param name="filter">An optional filter to apply to the list of open orders. If specified, only orders matching the filter criteria
    /// are included.</param>
    /// <param name="sort">An optional sort order to apply to the resulting list of open orders. If specified, orders are sorted according
    /// to the provided criteria.</param>
    /// <returns>An enumerable collection of open orders available to the specified courier, each represented as an
    /// OpenOrderInList object. The collection may be empty if no orders match the criteria.</returns>
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

    /// <summary>
    /// Filters the specified list of orders according to the given open order filter criteria.
    /// </summary>
    /// <param name="orders">The list of orders to be filtered.</param>
    /// <param name="filter">The filter criterion to apply to the orders.</param>
    /// <returns>A list of orders that match the specified filter criterion. If no orders match, the returned list may be empty.</returns>
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

    /// <summary>
    /// Sorts a list of open orders according to the specified sorting criteria.
    /// </summary>
    /// <remarks>Sorting by areal distance uses the store's configured latitude and longitude as the reference
    /// point. Sorting by expected actual delivery time uses the delivery's start time if available; otherwise, orders
    /// with no delivery information are placed last.</remarks>
    /// <param name="orders">The list of open orders to sort. Cannot be null.</param>
    /// <param name="sort">The sorting criteria to apply to the orders.</param>
    /// <returns>A new list of orders sorted according to the specified criteria. If an unrecognized sort option is provided, the
    /// original list is returned in its current order.</returns>
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