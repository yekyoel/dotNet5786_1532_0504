using BO;
using DalApi;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace Helpers;

internal static class CourierManager
{
    private static IDal s_dal = Factory.Get; //stage 4

    private static IDal dal => s_dal; 

    /// <summary>
    /// Type of user in the system.
    /// </summary>
    internal enum UserType
    {
        Admin,
        Courier,
        Unknown
    }
    /// <summary>
    /// Returns whether the given userId is the configured Admin, a Courier, or Unknown.
    /// </summary>
    /// <param name="userId">user identifier to check</param>
    /// <returns>UserType.Admin | UserType.Courier | UserType.Unknown</returns>
    internal static UserType GetUserType(string userId)
    {
        // Check admin id from configuration
        if (userId == "admin")
            return UserType.Admin;

        return UserType.Courier; // Default to Courier for any non-admin user
    }

    // Get list of couriers with optional filtering and sorting
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

    internal static BO.Courier fromDOToBO(DO.Courier doCourier) // Convert DO.Courier to BO.Courier
    {
        return new BO.Courier
        {
            Id = doCourier.Id,
            FullName = doCourier.FullName,
            PhoneNumber = doCourier.PhoneNum,
            Email = doCourier.Email,
            Password = string.Empty,
            IsActive = doCourier.IsActive,
            MaxDist = doCourier.MaxDist,
            OrderType = default,
            EmploymentStartDate = doCourier.DayStarted ?? default,
            TotalDelSuppliedOnTime = 0,
            TotalDelSuppliedLate = 0
        };
    }
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

    internal static string ToString(BO.Courier courier)
    {
        if (courier is null)
            return "BO.Courier: null";

        var started = courier.EmploymentStartDate == default ? "N/A" : courier.EmploymentStartDate.ToString("u");
        var maxDist = courier.MaxDist.HasValue ? courier.MaxDist.Value.ToString("F2") : "N/A";

        return $"BO.Courier: Id={courier.Id}; Name=\"{courier.FullName}\"; Phone=\"{courier.PhoneNumber}\"; Email=\"{courier.Email}\"; Active={courier.IsActive}; MaxDist={maxDist}; Started={started}; OrderType={courier.OrderType}; OnTime={courier.TotalDelSuppliedOnTime}; Late={courier.TotalDelSuppliedLate}";
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
