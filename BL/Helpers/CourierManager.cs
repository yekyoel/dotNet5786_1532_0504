using BO;
using DalApi;
using static Helpers.Tools;
namespace Helpers;

internal static class CourierManager
{
    private static IDal s_dal = Factory.Get; //stage 4
    internal static ObserverManager Observers = new(); // stage 5

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
                if (dal.Config.AdminId == id)
                    return UserType.Admin;

                // courier exists?
                var doCourier = dal.Courier.Read(id);
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
        var list = dal.Courier.ReadAll();

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
    internal static BO.Courier fromDOToBO(DO.Courier doCourier) // Convert DO.Courier to BO.Courier
    {
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
            TotalDelSuppliedOnTime = 0,
            TotalDelSuppliedLate = 0
        };
    }

    /// <summary>
    /// Converts a business object courier to its corresponding data object representation.
    /// </summary>
    /// <param name="boCourier">The business object courier to convert. Cannot be null.</param>
    /// <returns>A data object courier containing the values from the specified business object.</returns>
    internal static DO.Courier fromBOToDO(BO.Courier boCourier) // Convert BO.Courier to DO.Courier
    {
        return new DO.Courier
        (
            Id: boCourier.Id,
            FullName: boCourier.FullName,
            PhoneNum: boCourier.PhoneNumber,
            Email: boCourier.Email,
            IsActive: boCourier.IsActive,
            MaxDist: boCourier.MaxDist,
            PreferredShippingMethod: null,
            DayStarted: boCourier.EmploymentStartDate
        );
    }

    // String representation helpers for display / logging
    internal static string ToString(DO.Courier doCourier)
    {
        if (doCourier is null) 
            return "DO.Courier: null";

        return $"DO.Courier: Id={doCourier.Id}; Name=\"{doCourier.FullName}\"; Phone=\"{doCourier.PhoneNum}\"; Email=\"{doCourier.Email}\"; Active={doCourier.IsActive}; MaxDist={(doCourier.MaxDist.HasValue ? doCourier.MaxDist.Value.ToString("F2") : "N/A")}; Started={(doCourier.DayStarted.HasValue ? doCourier.DayStarted.Value.ToString("u") : "N/A")}";
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
        dal.Courier.Create(doCourier);
    }

    // Read courier by id
    internal static BO.Courier ReadCourier(int id)
    {
        var doCourier = dal.Courier.Read(id) ?? throw new KeyNotFoundException($"Courier with ID={id} not found.");
        return fromDOToBO(doCourier);
    }

    // Read all couriers with optional filtering
    internal static IEnumerable<BO.Courier> ReadAllCouriers(Func<BO.Courier, bool>? filter = null)
    {
        var doList = dal.Courier.ReadAll();
        var result = new List<BO.Courier>(capacity: doList is ICollection<DO.Courier> col ? col.Count : 0);

        foreach (var d in doList)
            result.Add(fromDOToBO(d));

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
        dal.Courier.Update(doCourier);
    }

    // Delete courier by id
    internal static void DeleteCourier(int id)
    {
        dal.Courier.Delete(id);
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
        try
        {
            if (newClock <= oldClock)
                return;

            // inactivity threshold: no deliveries within this span -> auto-disable courier
            TimeSpan inactivityThreshold = TimeSpan.FromDays(180);
            DateTime recentThreshold = newClock - inactivityThreshold;

            // Read once
            var deliveries = dal.Delivery.ReadAll();
            var couriers = dal.Courier.ReadAll();

            foreach (var c in couriers)
            {
                bool hasRecent = false;

                foreach (var d in deliveries)
                {
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
                        dal.Courier.Update(updated);
                    }
                    catch
                    {
                        // swallow individual DAL update failures and continue with others
                    }
                }
            }
        }
        catch
        {
            // swallow outer exceptions to avoid breaking clock update caller
        }
    }
}
