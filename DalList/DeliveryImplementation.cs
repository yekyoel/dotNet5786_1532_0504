namespace Dal;

using DalApi;
using DO;
using System.Collections.Generic;

internal class DeliveryImplementation : IDelivery
{
    public void Create(Delivery item)
    {
        int id1 = Config.NextOrderId;
        int id2 = Config.NextDeliveryId;
        Delivery newDel = item with { Id = id1, OrderId = id2 };
        DataSource.Deliveries.Add(newDel);
    }

    public void Delete(int id)
    {
        if (Read(id) == null)
            throw new NotImplementedException();
        DataSource.Deliveries.Remove(Read(id));
    }

    public void DeleteAll()
    {
            DataSource.Deliveries.Clear();
    }

    public Delivery? Read(int id)
    {
        foreach (Delivery itr in DataSource.Deliveries)
        {
            if (itr.Id == id)
                return itr;
        }
        return null;
    }

    public List<Delivery> ReadAll()
    {
        List<Delivery> list = new List<Delivery>(DataSource.Deliveries);
        return list;
    }

    public void Update(Delivery item)
    {
        if (Read(item.Id) == null)
            throw new NotImplementedException();
        Delete(item.Id);
        DataSource.Deliveries.Add(item);
    }
}