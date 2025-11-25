using BO;

namespace BlApi;

public interface ICourier
{
    string Login(String name);

    BO.CourierInList GetListOfCouriers(int userId, bool? mainFilter, CourierInListFilter? secondFilter);

    BO.Courier GetCourierDetails(int userId, int courierId);

    void UpdateCourierDetails(int userId, BO.Courier courier);

    void DeleteCourier(int userId, int courierId);

    void AddCourier(int userId, BO.Courier courier);

}
