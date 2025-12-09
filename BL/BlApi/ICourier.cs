namespace BlApi;

public interface ICourier
{
    public string Login(string userId);

    public IEnumerable<BO.CourierInList> GetListOfCouriers(int userId, bool? mainFilter, BO.CourierInListFilter? secondFilter);

    public BO.Courier GetCourierDetails(int userId, int courierId);

    public void UpdateCourierDetails(int userId, BO.Courier courier);

    public void DeleteCourier(int userId, int courierId);

    public void AddCourier(int userId, BO.Courier courier);

}
