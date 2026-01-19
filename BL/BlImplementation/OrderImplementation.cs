using BlApi;
using BO;
using Helpers;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BlImplementation;

internal class OrderImplementation : IOrder 
{
    public void AddObserver(Action listObserver) => OrderManager.Observers.AddListObserver(listObserver); //stage 5
    public void AddObserver(int id, Action observer) => OrderManager.Observers.AddObserver(id, observer); //stage 5
    public void RemoveObserver(Action listObserver) => OrderManager.Observers.RemoveListObserver(listObserver); //stage 5
    public void RemoveObserver(int id, Action observer) => OrderManager.Observers.RemoveObserver(id, observer); //stage 5


    /// <summary>
    /// Gets the total number of orders for each combination of order status and schedule status for the specified user.
    /// </summary>
    /// <remarks>The returned array contains one element for every possible combination of order status and
    /// schedule status, regardless of whether any orders exist for that combination. Callers can use <see
    /// cref="Enum.GetValues(Type)"/> on <see cref="BO.OrderStatus"/> and <see cref="BO.ScheduleStatus"/> to determine
    /// the number and order of statuses.</remarks>
    /// <param name="userId">The unique identifier of the user whose orders are counted.</param>
    /// <returns>An array of integers where each element represents the count of orders for a specific combination of <see
    /// cref="BO.OrderStatus"/> and <see cref="BO.ScheduleStatus"/>. The array is indexed by <c>(int)OrderStatus * N +
    /// (int)ScheduleStatus</c>, where <c>N</c> is the number of schedule status values.</returns>
    public int[] StatusTotal(int userId)
    {
        var adminId = AdminManager.GetConfig().AdminId;
        if (userId != adminId)
            throw new UnauthorizedAccessException("Only admin can access status totals.");

        var orders = Helpers.OrderManager.GetAllOrders(); // get all orders

        var orderStatuses = Enum.GetValues<BO.OrderStatus>(); // get all order statuses
        var scheduleStatuses = Enum.GetValues<BO.ScheduleStatus>(); // get all schedule statuses

        int scheduleStatusCount = scheduleStatuses.Length; // number of schedule statuses
        int[] result = new int[orderStatuses.Length * scheduleStatusCount]; // result array

        int Index(BO.OrderStatus os, BO.ScheduleStatus ss) =>
            (int)os * scheduleStatusCount + (int)ss; // calculate index in result array

        // mandatory grouping
        var grouped = orders.GroupBy(o => (o.OrderStatus, o.ScheduleStatus))
            .ToDictionary(g => g.Key, g => g.Count());

        // fill all enum combinations (including ones with 0 orders)
        foreach (var os in orderStatuses)
        {
            foreach (var ss in scheduleStatuses)
            {
                int i = Index(os, ss);
                if ((uint)i >= (uint)result.Length)
                    continue;

                result[i] = grouped.TryGetValue((os, ss), out var count) ? count : 0;
            }
        }

        return result;
    }

    /// <summary>
    /// Returns a delegate that selects a property from a <see cref="BO.OrderInList"/> instance based on the specified
    /// filter.
    /// </summary>
    /// <param name="filter">The property filter that determines which property of <see cref="BO.OrderInList"/> the selector will return.</param>
    /// <returns>A function that takes a <see cref="BO.OrderInList"/> and returns the value of the property specified by
    /// <paramref name="filter"/>; returns <see langword="null"/> if the filter does not match a known property.</returns>
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
            BO.OrderInListFilter.TotalTimeLeft => o => o.TotalTimeLeft,
            BO.OrderInListFilter.TotalCompletionTime => o => o.TotalCompletionTime,
            BO.OrderInListFilter.TotalDeliveries => o => o.TotalDeliveries,

            _ => _ => null
        };
    }

    /// <summary>
    /// Retrieves a filtered and sorted list of orders for a specified user.
    /// </summary>
    /// <remarks>Each order appears only once in the returned collection, representing the most recent
    /// delivery for that order. If both <paramref name="filter"/> and <paramref name="filterValue"/> are provided, only
    /// orders matching the specified property and value are included.</remarks>
    /// <param name="userId">The unique identifier of the user whose orders are to be retrieved.</param>
    /// <param name="filter">An optional filter specifying the order property to filter by. If <see langword="null"/>, no filtering is
    /// applied.</param>
    /// <param name="filterValue">The value to filter orders by. Must be provided if <paramref name="filter"/> is specified; otherwise, it is
    /// ignored.</param>
    /// <param name="sort">An optional sort criteria specifying the order property to sort by. If <see langword="null"/>, orders are sorted
    /// by status.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="BO.OrderInList"/> objects representing the user's orders after
    /// applying the specified filter and sort criteria. The collection is empty if no orders match the criteria.</returns>
   
    public IEnumerable<BO.OrderInList> GetListOfOrders(int userId, BO.OrderInListFilter? filter, object? filterValue, BO.OrderInListFilter? sort)
    {
        // recieve all orders
        IEnumerable<BO.OrderInList> orders =
            Helpers.OrderManager.GetAllOrders();

        // order by most recent delivery per order
        orders = orders
            .GroupBy(o => o.OrderId)
            .Select(g => g
                .OrderByDescending(o => o.DeliveryId) // TODO: adjust if needed
                .First());

        // filtering
        if (filter != null && filterValue != null)
        {
            var filterSelector = GetOrderPropertySelector(filter.Value);

            orders = orders.Where(o =>
            {
                var value = filterSelector(o);
                return value != null && value.Equals(filterValue);
            });
        }

        // sorting
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

    /// <summary>
    /// Retrieves the details of a specific order for the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user who owns the order.</param>
    /// <param name="orderId">The unique identifier of the order to retrieve.</param>
    /// <returns>The <see cref="BO.Order"/> object containing the details of the specified order.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if an order with the specified <paramref name="orderId"/> does not exist.</exception>
   
    public BO.Order GetOrderDetails(int userId, int orderId)
    {
        var order = Helpers.OrderManager.GetOrderById(orderId);

        if (order == null)
            throw new KeyNotFoundException($"Order with ID {orderId} not found");

        return order;
    }

    /// <summary>
    /// Updates the details of an existing order for the specified user.
    /// </summary>
    /// <remarks>This method updates the order information in the system. The order must already exist;
    /// otherwise, the update may fail.</remarks>
    /// <param name="userId">The identifier of the user associated with the order. Must correspond to a valid user in the system.</param>
    /// <param name="order">The order object containing the updated details. Cannot be <see langword="null"/>.</param>
    public void UpdateOrderDetails(int userId, BO.Order order)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7

        var adminId = AdminManager.GetConfig().AdminId;
        if (userId != adminId)
            throw new UnauthorizedAccessException("Only admin can update order details.");

        Helpers.OrderManager.UpdateOrder(order);
    }

    /// <summary>
    /// Cancels the specified order for a user.
    /// </summary>
    /// <param name="userId">The identifier of the user requesting the cancellation. Must correspond to a valid user with permission to
    /// cancel the order.</param>
    /// <param name="orderId">The identifier of the order to cancel.</param>
    public void CancelOrder(int userId, int orderId)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7

        var adminId = AdminManager.GetConfig().AdminId;
        if (userId != adminId)
            throw new UnauthorizedAccessException("Only admin can update order details.");
        Helpers.OrderManager.TryToCancelOrder(orderId);
    }

    /// <summary>
    /// Deletes the specified order for a user.
    /// </summary>
    /// <remarks>If the specified order does not exist or cannot be deleted, no exception is thrown and no
    /// action is taken.</remarks>
    /// <param name="userId">The identifier of the user who owns the order.</param>
    /// <param name="orderId">The identifier of the order to delete.</param>
    public void DeleteOrder(int userId, int orderId)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7

        var adminId = AdminManager.GetConfig().AdminId;
        if (userId != adminId)
            throw new UnauthorizedAccessException("Only admin can update order details.");

        Helpers.OrderManager.TryToDeleteOrder(orderId);
    }

    /// <summary>
    /// Adds a new order for the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user for whom the order is being added.</param>
    /// <param name="order">The order to add. Cannot be <see langword="null"/>.</param>
   
    public  Task AddOrder(int userId, BO.Order order)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7

        return OrderManager.AddOrder(order);
    }

    /// <summary>
    /// Marks the specified delivery as complete if the user is authorized.
    /// </summary>
    /// <param name="userId">The identifier of the user attempting to complete the delivery. Must match the courier assigned to the delivery.</param>
    /// <param name="courierId">The identifier of the courier assigned to the delivery.</param>
    /// <param name="deliveryId">The identifier of the delivery to mark as complete.</param>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="userId"/> does not match <paramref name="courierId"/>, indicating the user is not
    /// authorized to complete the delivery.</exception>
   
    public void OrderComplete(int userId, int courierId, int deliveryId ,BO.CompletionType compType)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7

        
        if (userId == courierId)
        {
            Helpers.DeliveryManager.CompleteDelivery(deliveryId, compType);
        }
        else
        {
            throw new InvalidOperationException("User is not authorized to complete this delivery.");
        }
    }

    /// <summary>
    /// Assigns the specified order to the courier if the user is authorized.
    /// </summary>
    /// <param name="userId">The identifier of the user attempting to assign the order.</param>
    /// <param name="courierId">The identifier of the courier to whom the order will be assigned. Must match <paramref name="userId"/> for the
    /// operation to succeed.</param>
    /// <param name="orderId">The identifier of the order to assign.</param>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="userId"/> does not match <paramref name="courierId"/>, indicating the user is not
    /// authorized to assign the order.</exception>
    
    public Task ChooseOrderAsync(int userId, int courierId, int orderId)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7

        if (userId == courierId)
        {
            return Helpers.OrderManager.AssignOrderToCourierAsync(orderId, courierId);
        }
        else
        {
            throw new InvalidOperationException("User is not authorized to choose this order.");
        }
    }

    /// <summary>
    /// Retrieves a collection of completed deliveries assigned to a specified courier.
    /// </summary>
    /// <param name="userId">The identifier of the user requesting the completed deliveries. This is used for access control and auditing
    /// purposes.</param>
    /// <param name="courierId">The identifier of the courier whose completed deliveries are to be retrieved.</param>
    /// <param name="filter">An optional filter to apply to the list of completed deliveries. If <see langword="null"/>, no filtering is
    /// applied.</param>
    /// <param name="sort">An optional sort specification for ordering the results. If <see langword="null"/>, the default sort order is
    /// used.</param>
    /// <returns>An enumerable collection of <see cref="BO.ClosedDeliveryInList"/> objects representing the completed deliveries
    /// for the specified courier. The collection is empty if no completed deliveries are found.</returns>
   
    public  Task<IEnumerable<BO.ClosedDeliveryInList>> GetCompletedCourierDeliveriesAsync(int userId, int courierId, BO.ClosedDeliveryInListFilter? filter, BO.ClosedDeliveryInListFilter? sort)
    {
        return Helpers.OrderManager.GetClosedDeliveriesAsync(courierId, filter, sort);
    }

    /// <summary>
    /// Retrieves a collection of open orders that are available for assignment to the specified courier.
    /// </summary>
    /// <param name="userId">The identifier of the user requesting the available orders. This is typically used for authorization or auditing
    /// purposes.</param>
    /// <param name="courierId">The identifier of the courier for whom to retrieve available orders. Only orders that can be assigned to this
    /// courier are returned.</param>
    /// <param name="filter">An optional filter to apply to the list of open orders. If specified, only orders matching the filter criteria
    /// are included; otherwise, all available orders are returned.</param>
    /// <param name="sort">An optional sort specification that determines the order in which the results are returned. If <see
    /// langword="null"/>, the default sort order is used.</param>
    /// <returns>An enumerable collection of <see cref="BO.OpenOrderInList"/> objects representing the open orders available to
    /// the specified courier. The collection is empty if no matching orders are found.</returns>

    public  Task<IEnumerable<BO.OpenOrderInList>> GetAvailableOrdersForCourierAsync(int userId, int courierId, BO.OpenOrderInListFilter? filter, BO.OpenOrderInListFilter? sort)
    {
        return Helpers.OrderManager.GetOpenOrdersAsync(courierId, filter, sort);
    }
}
