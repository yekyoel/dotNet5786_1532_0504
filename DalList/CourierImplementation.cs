namespace Dal;

using DalApi;
using DO;
using System.Collections.Generic;

internal class CourierImplementation : ICourier
{
    public void Create(Courier item)
    {
        if(Read(item.Id) != null)
            throw new Exception($"Courier with ID={item.Id} already exists"); // ID must be unique
        DataSource.Couriers.Add(item);
    }

    public void Delete(int id)
    {
        if (Read(id) == null)
            throw new Exception($"Courier with ID={id} doesn't exist"); //  cannot delete non-existing object
        DataSource.Couriers.Remove(Read(id));
    }

    public void DeleteAll()
    {
        foreach (Courier itr in DataSource.Couriers.ToArray()) // to avoid modifying collection during iteration
        {
            DataSource.Couriers.Remove(itr); // remove each courier
        }
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
            throw new Exception($"Courier with ID={item.Id} doesn't exist"); // cannot update non-existing object
        Delete(item.Id);
        DataSource.Couriers.Add(item);
    }
}