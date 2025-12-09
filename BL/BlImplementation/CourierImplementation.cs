namespace BlImplementation;

using BlApi;
using Helpers;
using static Helpers.Tools;
internal class CourierImplementation : ICourier
{
    public void AddCourier(int userId, BO.Courier courier) // add a new courier to the system
    {
        if (courier == null)
            throw new ArgumentNullException(nameof(courier));

        // Delegate creation to CourierManager (handles DO/BO mapping and DAL call)
        CourierManager.CreateCourier(courier);
    }

    public void DeleteCourier(int userId, int courierId) // delete a courier from the system
    {
        var adminId = AdminManager.GetConfig().AdminId;
        if (userId != adminId)
            throw new UnauthorizedAccessException("Only admin may delete couriers.");

        // Use manager wrapper
        CourierManager.DeleteCourier(courierId);
    }

    public BO.Courier GetCourierDetails(int userId, int courierId) // get detailed information about a specific courier
    {
        if (courierId <= 0)
            throw new ArgumentException("courierId must be positive", nameof(courierId));

        var adminId = AdminManager.GetConfig().AdminId;

        // Allow admin or the courier themselves to request details
        if (userId != adminId && userId != courierId)
            throw new UnauthorizedAccessException("Only admin or the courier may view these details.");

        // Use manager wrapper that returns BO.Courier
        return CourierManager.ReadCourier(courierId);
    }

    public IEnumerable<BO.CourierInList> GetListOfCouriers(int userId, bool? mainFilter, BO.CourierInListFilter? secondFilter)
    {
        // Reuse existing manager method for DO-level filtering/sorting
        var doCouriers = CourierManager.GetCouriers(mainFilter, sortBy: secondFilter);

        var result = new List<BO.CourierInList>();
        foreach (var d in doCouriers)
        {
            // Convert DO->BO then map to CourierInList (uses manager helper)
            var bo = CourierManager.fromDOToBO(d);
            result.Add(new BO.CourierInList
            {
                CourierId = bo.Id,
                FullName = bo.FullName,
                IsActive = bo.IsActive,
                TypeOrder = FindScheduleStatusType(bo),
                EmploymentStartDate = bo.EmploymentStartDate,
                TotalDelSuppliedOnTime = bo.TotalDelSuppliedOnTime,
                TotalLateDelSupplied = bo.TotalDelSuppliedLate,
                OrderId = 0 // Delivery/current-order not present in DO.Courier; compute if needed
            });
        }

        return result;
    }

    public string Login(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("userId cannot be null or whitespace", nameof(userId));

        var userType = CourierManager.GetUserType(userId);

        return userType switch
        {
            CourierManager.UserType.Admin => "Admin",
            CourierManager.UserType.Courier => "Courier",
            _ => throw new UnauthorizedAccessException("User not found or password incorrect")
        };
    }

    public void UpdateCourierDetails(int userId, BO.Courier courier)
    {
        if (courier is null)
            throw new ArgumentNullException(nameof(courier));

        var adminId = AdminManager.GetConfig().AdminId;
        if (userId != adminId && userId != courier.Id)
            throw new UnauthorizedAccessException("Only admin or the courier may update details.");

        // Delegate update to manager (handles mapping + DAL)
        CourierManager.UpdateCourier(courier);
    }
}
