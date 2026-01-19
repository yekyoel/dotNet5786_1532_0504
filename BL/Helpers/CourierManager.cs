using BO;
using DalApi;
using static Helpers.Tools;
namespace Helpers;

internal static class CourierManager
{
    private static IDal s_dal = Factory.Get; //stage 4
    internal static ObserverManager Observers = new(); // stage 5
    private static readonly AsyncMutex s_periodicMutex = new(); //stage 7
    private static IDal dal => s_dal; 

    /// <summary>
    /// Type of user in the system.
    /// </summary>
    internal enum UserType
    {
        Admin,
        Courier,
    }

    /// <summary>
    /// Returns whether the given userId is the configured Admin, a Courier, or Unknown.
    /// </summary>
    /// <param name="userId">user identifier to check</param>
    /// <returns>UserType.Admin | UserType.Courier | UserType.Unknown</returns>
    internal static UserType GetUserType(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new BlDoesNotExistException("User does not exist");

        // allow explicit "admin" literal
        if (userId.Equals("admin", StringComparison.OrdinalIgnoreCase))
            return UserType.Admin;

        // try numeric id
        if (int.TryParse(userId, out var id))
        {
            try
            {
                // admin configured id in DAL takes precedence
                int adminId;
                lock (AdminManager.BlMutex)
                    adminId = dal.Config.AdminId;
                if (adminId == id)
                    return UserType.Admin;

                // courier exists?
                DO.Courier? doCourier;
                lock (AdminManager.BlMutex)
                    doCourier = dal.Courier.Read(id);
                if (doCourier is not null)
                    return UserType.Courier;
            }
            catch
            {
                throw new BlDoesNotExistException("User does not exist");
            }
        }

        throw new BlDoesNotExistException("User does not exist");
    }


    /// <summary>
    /// Retrieves a collection of couriers, optionally filtered by active status and sorted according to the specified
    /// criteria.
    /// </summary>
    /// <param name="isActive">If specified, filters the results to include only couriers whose active status matches this value. If null, no
    /// filtering by active status is applied.</param>
    /// <param name="sortBy">An optional value specifying the property by which to sort the returned couriers. If null, the original order is
    /// preserved.</param>
    /// <returns>An enumerable collection of couriers that match the specified filter and sorting criteria.</returns>
    internal static IEnumerable<DO.Courier> GetCouriers( bool? isActive, CourierInListFilter? sortBy)
    {
        IEnumerable<DO.Courier> list;
        lock (AdminManager.BlMutex)
            list = dal.Courier.ReadAll();

        // filter
        if (isActive is not null)
            list = list.Where(c => c.IsActive == isActive); // filter by active status

        // sort
        if (sortBy is not null)
        {
            list = sortBy switch
            {
                CourierInListFilter.CourierId => list.OrderBy(c => c.Id),
                CourierInListFilter.FullName => list.OrderBy(c => c.FullName),
                CourierInListFilter.IsActive => list.OrderBy(c => c.IsActive),
                CourierInListFilter.EmploymentStartDate => list.OrderBy(c => c.DayStarted),
                _ => list
            };
        }

        return list;
    }

   /// <summary>
   /// Converts a data object representing a courier to its corresponding business object representation.
   /// </summary>
   /// <param name="doCourier">The data object containing courier information to convert. Cannot be null.</param>
   /// <returns>A business object representing the courier with values mapped from the provided data object.</returns>
    internal static async Task<BO.Courier> fromDOToBO(DO.Courier doCourier) // Convert DO.Courier to BO.Courier
    {
        DO.Delivery? delivery;
        lock (AdminManager.BlMutex)
        {
            delivery = dal.Delivery.ReadAll()
            .FirstOrDefault(d => d.CourierId == doCourier.Id && d.DeliveryEndTime is null);
        }

        DO.Order? order;
        if (delivery is null)
            order = null;
        else
            lock (AdminManager.BlMutex)
            {
                order = dal.Order.Read(delivery.OrderId); 
            }

        // Calculate delivery statistics
        int totalOnTime = 0;
        int totalLate = 0;

        // Iterate over all completed deliveries for this courier to calculate totals
        IEnumerable<DO.Delivery?> completedDeliveries;
        lock (AdminManager.BlMutex)
        {
            completedDeliveries = dal.Delivery.ReadAll()
            .Where(d => d.CourierId == doCourier.Id && d.DeliveryEndTime != null);
        }

        foreach (var d in completedDeliveries)
        {
            if (d == null) continue;
            DO.Order? ord;
            lock (AdminManager.BlMutex)
                ord = dal.Order.Read(d.OrderId);
            if (ord != null)
            {
                var status = Tools.FindScheduleStatusType(ord);
                if (status == BO.ScheduleStatus.Late)
                    totalLate++;
                else if (status == BO.ScheduleStatus.OnTime)
                    totalOnTime++;
            }
        }

        // Calculate active order in progress
        BO.OrderInProgress orderInProg;

        // If there's an active delivery and order, populate OrderInProgress
        if (delivery is not null && order is not null)
        {
            // Calculate aerial distance from store to customer
            var cfg = AdminManager.GetConfig();
            var storeLat = cfg?.Latitude ?? 0.0;
            var storeLon = cfg?.Longitude ?? 0.0;

            // Aerial distance between store and order delivery location
            var aerialDistance = Tools.GetAerialDistanceKm(storeLat, storeLon, order.Latitude, order.Longitude);
            var actualDistance = await Tools.GetTotalDistance(order);
            var expectedDeliveryTime = Tools.CalculateExpectedDeliveryTime(order, delivery);
            var maxDeliveryTime = (order.StartTimeForOrdering ?? (cfg?.Clock ?? DateTime.Now))
                .Add(cfg?.MaxDelTime ?? TimeSpan.Zero);

            // Populate OrderInProgress details
            orderInProg = new BO.OrderInProgress
            {
                DeliveryId = delivery.Id,
                OrderId = order.Id,
                OrderType = Tools.SwitchOrderTypeTOBO(order) ?? BO.OrderType.Pizza,
                Description = order.Description,
                DeliveryAddress = order.FullAdd ?? string.Empty,
                ArealDistance = aerialDistance,
                ActualDistance = actualDistance,
                CustomerName = order.CustFullName ?? string.Empty,
                CustomerNumber = order.CusNum ?? string.Empty,
                OrderPlacedTime = order.StartTimeForOrdering ?? (cfg?.Clock ?? DateTime.Now),
                DeliveryTime = delivery.DeliveryStartTime ?? (cfg?.Clock ?? DateTime.Now),
                ExpectedDeliveryTime = expectedDeliveryTime ?? maxDeliveryTime,
                MaxDeliveryTime = maxDeliveryTime,
                OrderStats = Tools.FindOrderStatusType(order) ?? BO.OrderStatus.Open,
                ScheduleStat = Tools.FindScheduleStatusType(order) ?? BO.ScheduleStatus.OnTime,
                TotalTimeLeft = Tools.CalculateTotalTimeLeft(order, delivery)
            };
        }
        else // No active delivery/order found for this courier
        {
            // No active delivery/order found for this courier -> still initialize (non-null)
            orderInProg = new BO.OrderInProgress
            {
                DeliveryId = 0,
                OrderId = 0,
                OrderType = BO.OrderType.Pizza,
                Description = null,
                DeliveryAddress = string.Empty,
                ArealDistance = 0,
                ActualDistance = null,
                CustomerName = string.Empty,
                CustomerNumber = string.Empty,
                OrderPlacedTime = DateTime.MinValue,
                DeliveryTime = DateTime.MinValue,
                ExpectedDeliveryTime = DateTime.MinValue,
                MaxDeliveryTime = DateTime.MinValue,
                OrderStats = BO.OrderStatus.Open,
                ScheduleStat = BO.ScheduleStatus.OnTime,
                TotalTimeLeft = TimeSpan.Zero
            };
        }

        return new BO.Courier
        {
            Id = doCourier.Id,
            FullName = doCourier.FullName,
            PhoneNumber = doCourier.PhoneNum,
            Email = doCourier.Email,
            IsActive = doCourier.IsActive,
            MaxDist = doCourier.MaxDist,
            ShippingMethod = FindType(doCourier),
            EmploymentStartDate = doCourier.DayStarted,
            TotalDelSuppliedOnTime = totalOnTime,
            TotalDelSuppliedLate = totalLate,
            OrderInProg = orderInProg
        };
    }

    internal static BO.Courier fromDOToBOForOrderInList(DO.Courier doCourier) // Convert DO.Courier to BO.Courier non -async version
    {
        DO.Delivery? delivery;
        lock (AdminManager.BlMutex)
        {
            delivery = dal.Delivery.ReadAll()
            .FirstOrDefault(d => d.CourierId == doCourier.Id && d.DeliveryEndTime is null);
        }

        DO.Order? order;
        if (delivery is null)
            order = null;
        else
            lock (AdminManager.BlMutex)
            {
                order = dal.Order.Read(delivery.OrderId);
            }

        // Calculate delivery statistics
        int totalOnTime = 0;
        int totalLate = 0;

        // Iterate over all completed deliveries for this courier to calculate totals
        IEnumerable<DO.Delivery?> completedDeliveries;
        lock (AdminManager.BlMutex)
        {
            completedDeliveries = dal.Delivery.ReadAll()
            .Where(d => d.CourierId == doCourier.Id && d.DeliveryEndTime != null);
        }

        foreach (var d in completedDeliveries)
        {
            if (d == null) continue;
            DO.Order? ord;
            lock (AdminManager.BlMutex)
                ord = dal.Order.Read(d.OrderId);
            if (ord != null)
            {
                var status = Tools.FindScheduleStatusType(ord);
                if (status == BO.ScheduleStatus.Late)
                    totalLate++;
                else if (status == BO.ScheduleStatus.OnTime)
                    totalOnTime++;
            }
        }

        var orderInProg = new BO.OrderInProgress();

        return new BO.Courier
        {
            Id = doCourier.Id,
            FullName = doCourier.FullName,
            PhoneNumber = doCourier.PhoneNum,
            Email = doCourier.Email,
            IsActive = doCourier.IsActive,
            MaxDist = doCourier.MaxDist,
            ShippingMethod = FindType(doCourier),
            EmploymentStartDate = doCourier.DayStarted,
            TotalDelSuppliedOnTime = totalOnTime,
            TotalDelSuppliedLate = totalLate,
            OrderInProg = orderInProg
        };
    }


    /// <summary>
    /// Converts a business object courier to its corresponding data object representation.
    /// </summary>
    /// <param name="boCourier">The business object courier to convert. Cannot be null.</param>
    /// <returns>A data object courier containing the values from the specified business object.</returns>
    internal static DO.Courier fromBOToDO(BO.Courier boCourier) // Convert BO.Courier to DO.Courier
    {
        // Convert ShippingMethod from BO to DO
        DO.ShippingMethod? doMethod = boCourier.ShippingMethod switch
        {
            BO.ShippingMethod.Car => DO.ShippingMethod.Car,
            BO.ShippingMethod.Motorcycle => DO.ShippingMethod.Motorcycle,
            BO.ShippingMethod.Bike => DO.ShippingMethod.Bike,
            BO.ShippingMethod.OnFoot => DO.ShippingMethod.OnFoot,
            _ => null
        };

        return new DO.Courier
        (
            Id: boCourier.Id,
            FullName: boCourier.FullName,
            PhoneNum: boCourier.PhoneNumber,
            Email: boCourier.Email,
            IsActive: boCourier.IsActive,
            MaxDist: boCourier.MaxDist,
            PreferredShippingMethod: doMethod,
            DayStarted: boCourier.EmploymentStartDate
        );
    }


    /// <summary>
    /// Returns a string that represents the specified courier, including key properties such as ID, name, contact
    /// information, employment status, and delivery statistics.
    /// </summary>
    /// <param name="courier">The courier to represent as a string. Can be null.</param>
    /// <returns>A string containing the courier's details. If <paramref name="courier"/> is null, returns a string indicating
    /// that the courier is null.</returns>
    internal static string ToString(BO.Courier courier)
    {
        if (courier is null)
            return "BO.Courier: null";

        var started = courier.EmploymentStartDate == null || !courier.EmploymentStartDate.HasValue
            ? "N/A"
            : courier.EmploymentStartDate.Value.ToString("u");
        var maxDist = courier.MaxDist.HasValue ? courier.MaxDist.Value.ToString("F2") : "N/A";

        // Removed OrderType property, replaced with ShippingMethod (which exists in BO.Courier)
        return $"BO.Courier: Id={courier.Id}; Name=\"{courier.FullName}\"; Phone=\"{courier.PhoneNumber}\"; Email=\"{courier.Email}\"; Active={courier.IsActive}; MaxDist={maxDist}; Started={started}; ShippingMethod={courier.ShippingMethod}; OnTime={courier.TotalDelSuppliedOnTime}; Late={courier.TotalDelSuppliedLate}";
    }

    // ---------- CRUD helpers for BL (wrap DAL + DO/BO mapping) ----------

    // Create new courier
    internal static void CreateCourier(BO.Courier courier)
    {
        if (courier is null)
            throw new ArgumentNullException(nameof(courier));

        var doCourier = fromBOToDO(courier);
        lock (AdminManager.BlMutex)
            dal.Courier.Create(doCourier);


        Observers.NotifyItemUpdated(courier.Id); //stage 5
        Observers.NotifyListUpdated(); //stage 5
    }

    // Read courier by id
    internal static async Task<BO.Courier> ReadCourier(int id)
    {
        DO.Courier? doCourier;
        lock (AdminManager.BlMutex)
             doCourier = dal.Courier.Read(id);
             
        if (doCourier == null) throw new KeyNotFoundException($"Courier with ID={id} not found.");
        return await fromDOToBO(doCourier);
    }

    // Read all couriers with optional filtering
    internal static async Task<IEnumerable<BO.Courier>> ReadAllCouriers(Func<BO.Courier, bool>? filter = null)
    {
        IEnumerable<DO.Courier?> doList;
        lock (AdminManager.BlMutex)
            doList = dal.Courier.ReadAll();

        var result = new List<BO.Courier>(capacity: doList is ICollection<DO.Courier> col ? col.Count : 0);

        foreach (var d in doList)
             result.Add(await fromDOToBO(d));

        if (filter is null)
            return result;

        var filtered = new List<BO.Courier>();
        foreach (var b in result)
            if (filter(b))
                filtered.Add(b);

        return filtered;
    }

    // Update existing courier
    internal static void UpdateCourier(BO.Courier courier)
    {
        if (courier is null)
            throw new ArgumentNullException(nameof(courier));

        var doCourier = fromBOToDO(courier);
        lock (AdminManager.BlMutex)
            dal.Courier.Update(doCourier);
        Observers.NotifyItemUpdated(courier.Id); //stage 5
        Observers.NotifyListUpdated();  //stage 5
    }

    // Delete courier by id
    internal static void DeleteCourier(int id)
    {
        lock (AdminManager.BlMutex)
            dal.Courier.Delete(id);
        Observers.NotifyItemUpdated(id); //stage 5
        Observers.NotifyListUpdated();  //stage 5
    }

    /// <summary>
    /// Retrieves the full name of a courier by their unique identifier.
    /// </summary>
    /// <param name="courierId">The unique identifier of the courier whose name is to be retrieved. If 0 or invalid, returns "N/A".</param>
    /// <returns>The full name of the courier if found; otherwise, "N/A".</returns>
    internal static string GetCourierNameById(int courierId)
    {
        if (courierId <= 0)
            return "N/A";

        try
        {
            DO.Courier? doCourier;
            lock (AdminManager.BlMutex)
                doCourier = dal.Courier.Read(courierId);
            return doCourier?.FullName ?? "N/A";
        }
        catch
        {
            return "N/A";
        }
    }

    /// <summary>
    /// Periodic updates specific to couriers. Called after clock changes.
    /// Current policy:
    /// - If a courier has no deliveries (start or end) within the inactivity threshold, mark them inactive.
    /// - Keep method lightweight and resilient: exceptions are swallowed so clock update will not fail.
    /// Adjust threshold or logic to fit business rules.
    /// </summary>
    internal static void PeriodicCouriersUpdates(DateTime oldClock, DateTime newClock)
    {
        if (s_periodicMutex.CheckAndSetInProgress())
            return;
        try
        {
            if (newClock <= oldClock)
                return;

            // inactivity threshold: no deliveries within this span -> auto-disable courier
            TimeSpan inactivityThreshold = TimeSpan.FromDays(180);
            DateTime recentThreshold = newClock - inactivityThreshold;

            // Read once
            List<DO.Delivery?> deliveries;
            List<DO.Courier?> couriers;
            lock (AdminManager.BlMutex)
            {
                deliveries = dal.Delivery.ReadAll().ToList();
                couriers = dal.Courier.ReadAll().ToList();
            }

            var updatedCouriers = new List<int>();

            foreach (var c in couriers)
            {
                if (c == null) continue;
                bool hasRecent = false;

                foreach (var d in deliveries)
                {
                    if (d == null) continue;
                    if (d.CourierId != c.Id)
                        continue;

                    // consider delivery end time, otherwise start time
                    DateTime reference = d.DeliveryEndTime ?? d.DeliveryStartTime ?? DateTime.MinValue;
                    if (reference >= recentThreshold)
                    {
                        hasRecent = true;
                        break;
                    }
                }

                // If courier is active but has no recent deliveries, mark inactive.
                if (!hasRecent && c.IsActive)
                {
                    try
                    {
                        var updated = c with { IsActive = false };
                        lock (AdminManager.BlMutex)
                            dal.Courier.Update(updated);
                        
                        updatedCouriers.Add(c.Id);
                    }
                    catch
                    {
                        // swallow individual DAL update failures and continue with others
                    }
                }
            }

            foreach (var id in updatedCouriers)
            {
                Observers.NotifyItemUpdated(id);
            }
            if (updatedCouriers.Count > 0)
                Observers.NotifyListUpdated();
        }
        catch
        {
            // swallow outer exceptions to avoid breaking clock update caller
        }
        finally
        {
            s_periodicMutex.UnsetInProgress();
        }
    }


    internal static async Task SimulateCourierActivity()
    {

        var activeCouriers = (await ReadAllCouriers(c => c.IsActive)).ToList();
        var config = AdminManager.GetConfig();
        var clock = config?.Clock ?? DateTime.Now;

        foreach (var courier in activeCouriers)
        {
            // Case 1: Courier has NO order in progress
            if (courier.OrderInProg == null || courier.OrderInProg.OrderId == 0)
            {
                // Probability 0.15 to be available/check for orders
                if (Random.Shared.NextDouble() < 0.15)
                {
                    try
                    {
                        // Get available orders for this courier
                        var potentialOrders = await OrderManager.GetOpenOrdersAsync(courier.Id, null, null);
                        var ordersList = potentialOrders.ToList();

                        if (ordersList.Count > 0)
                        {
                            // Randomly choose one order
                            var selectedOrder = ordersList[Random.Shared.Next(ordersList.Count)];

                            // Probability 50% to actually pick (assign) the order
                            if (Random.Shared.NextDouble() < 0.5)
                            {
                                await OrderManager.AssignOrderToCourierAsync(selectedOrder.OrderId, courier.Id);
                            }
                        }
                    }
                    catch
                    {
                        // Ignore errors during simulation to keep it running
                    }
                }
            }
            // Case 2: Courier HAS an order in progress
            else
            {
                var orderInProg = courier.OrderInProg;
                
                // Calculate time elapsed since delivery started
                var startTime = orderInProg.DeliveryTime;
                var timePassed = clock - startTime;


                double dist = orderInProg.ActualDistance ?? orderInProg.ArealDistance;
                if (dist <= 0) dist = 5; // fallback min distance

                double estimatedMinutes = (dist / 40.0) * 60.0; 
                // Random buffer: 10 to 30 minutes
                double bufferMinutes = Random.Shared.Next(5, 10);
                
                double thresholdMinutes = estimatedMinutes + bufferMinutes;

                if (timePassed.TotalMinutes >= thresholdMinutes)
                {
                    // "Enough time" passed -> Complete the order
                    // Vary completion type
                    double rnd = Random.Shared.NextDouble();
                    DO.CompletionType completionType;
                    
                    if (rnd < 0.90) completionType = DO.CompletionType.Delivered; // Most likely
                    else if (rnd < 0.95) completionType = DO.CompletionType.Refused;
                    else completionType = DO.CompletionType.Failed; // Rare

                    try
                    {
                        var compType = Tools.SwitchCompletionTypeTOBO(completionType);
                        DeliveryManager.CompleteDelivery(orderInProg.DeliveryId, compType);
                    }
                    catch
                    {
                         // Ignore errors
                    }
                }
                else
                {
                    // Not enough time passed -> 10% chance to cancel handling
                    if (Random.Shared.NextDouble() < 0.10)
                    {
                        try
                        {
                             OrderManager.TryToCancelOrder(orderInProg.OrderId);
                        }
                        catch
                        {
                             // Ignore errors
                        }
                    }
                }
            }
        }
    }
}
