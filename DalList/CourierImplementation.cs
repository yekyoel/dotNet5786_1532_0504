namespace Dal;

using DalApi;
using DO;
using System.Collections.Generic;

public class CourierImplementation : ICourier
{
    public void Create(Courier item)
    {
        if(Read(item.Id) != null)
            throw new NotImplementedException();
        DataSource.Couriers.Add(item);
    }

    public void Delete(int id)
    {
        if (Read(id) == null)
            throw new NotImplementedException();
        DataSource.Couriers.Remove(Read(id));
    }

    public void DeleteAll()
    {
        foreach (Courier itr in DataSource.Couriers)
            DataSource.Couriers.Clear();

    }

    public Courier? Read(int id)
    {
        foreach (Courier itr in DataSource.Couriers)
        {
            if (itr.Id == id)
                return itr;
        }
        return null;
    }

    public List<Courier> ReadAll()
    {
        List<Courier> list = new List<Courier>(DataSource.Couriers);
        return list;
    }

    public void Update(Courier item)
    {
        if (Read(item.Id) == null)
            throw new NotImplementedException();
        Delete(item.Id);
        DataSource.Couriers.Add(item);
    }
}