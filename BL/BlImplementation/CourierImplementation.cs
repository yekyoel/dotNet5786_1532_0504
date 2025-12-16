namespace BlImplementation;

using BlApi;
using Helpers;
using static Helpers.Tools;
internal class CourierImplementation : ICourier
{
    public void AddObserver(Action listObserver) => CourierManager.Observers.AddListObserver(listObserver); //stage 5
    public void AddObserver(int id, Action observer) => CourierManager.Observers.AddObserver(id, observer); //stage 5
    public void RemoveObserver(Action listObserver) => CourierManager.Observers.RemoveListObserver(listObserver); //stage 5
    public void RemoveObserver(int id, Action observer) => CourierManager.Observers.RemoveObserver(id, observer); //stage 5


    /// <summary>
    /// Adds a new courier to the system for the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user associated with the courier.</param>
    /// <param name="courier">The <see cref="BO.Courier"/> object containing the details of the courier to add. Cannot be <see
    /// langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="courier"/> is <see langword="null"/>.</exception>
    public void AddCourier(int userId, BO.Courier courier) // add a new courier to the system
    {
        if (courier == null)
            throw new ArgumentNullException(nameof(courier));

        // Delegate creation to CourierManager (handles DO/BO mapping and DAL call)
        CourierManager.CreateCourier(courier);
    }

    /// <summary>
    /// Deletes a courier from the system.
    /// </summary>
    /// <param name="userId">The identifier of the user requesting the deletion. Must be the administrator's user ID.</param>
    /// <param name="courierId">The identifier of the courier to delete.</param>
    /// <exception cref="UnauthorizedAccessException">Thrown if <paramref name="userId"/> does not match the administrator's user ID.</exception>
    public void DeleteCourier(int userId, int courierId) // delete a courier from the system
    {
        var adminId = AdminManager.GetConfig().AdminId;
        if (userId != adminId)
            throw new UnauthorizedAccessException("Only admin may delete couriers.");

        // Use manager wrapper
        CourierManager.DeleteCourier(courierId);
    }

    /// <summary>
    /// Retrieves detailed information about a specific courier.
    /// </summary>
    /// <param name="userId">The identifier of the user requesting the courier details. Must be either the administrator's user ID or the
    /// courier's own user ID.</param>
    /// <param name="courierId">The identifier of the courier whose details are to be retrieved. Must be a positive integer.</param>
    /// <returns>A <see cref="BO.Courier"/> object containing detailed information about the specified courier.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="courierId"/> is less than or equal to zero.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if <paramref name="userId"/> is not the administrator or the courier whose details are being requested.</exception>
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

    /// <summary>
    /// Retrieves a collection of couriers with summary information, filtered and sorted according to the specified
    /// criteria.
    /// </summary>
    /// <param name="userId">The identifier of the user requesting the list. This parameter may be used to determine access permissions or
    /// personalize the results.</param>
    /// <param name="mainFilter">An optional filter indicating whether to include only couriers matching a primary condition. If <see
    /// langword="null"/>, no primary filtering is applied.</param>
    /// <param name="secondFilter">An optional secondary filter or sort order to apply to the list of couriers. If <see langword="null"/>, the
    /// default sorting is used.</param>
    /// <returns>An enumerable collection of <see cref="BO.CourierInList"/> objects representing couriers that match the
    /// specified filters. The collection is empty if no couriers meet the criteria.</returns>
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
                // derive the food type most associated with this courier
                TypeOrder = FindCourierOrderType(d),
                EmploymentStartDate = bo.EmploymentStartDate,
                TotalDelSuppliedOnTime = bo.TotalDelSuppliedOnTime,
                TotalLateDelSupplied = bo.TotalDelSuppliedLate,
                OrderId = 0 // Delivery/current-order not present in DO.Courier; compute if needed
            });
        }

        return result;
    }

    /// <summary>
    /// Authenticates a user by their user ID and returns their user role if authentication is successful.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to authenticate. Cannot be <see langword="null"/>, empty, or consist only of
    /// white-space characters.</param>
    /// <returns>A string representing the user's role. Returns "Admin" if the user is an administrator, or "Courier" if the user
    /// is a courier.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="userId"/> is <see langword="null"/>, empty, or consists only of white-space
    /// characters.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the user is not found or the password is incorrect.</exception>
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

    /// <summary>
    /// Updates the details of a courier with the specified information.
    /// </summary>
    /// <param name="userId">The identifier of the user requesting the update. Must be either the administrator or the courier being updated.</param>
    /// <param name="courier">The <see cref="BO.Courier"/> object containing the updated courier details. Cannot be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="courier"/> is <see langword="null"/>.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if <paramref name="userId"/> is neither the administrator nor the courier being updated.</exception>
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
