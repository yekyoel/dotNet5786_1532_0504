namespace Dal;

using DalApi;
using DO;
using System.Collections.Generic;

public class OrderImplementation : IOrder
{ 
    public void Create(Order item)
    {
        int id = Config.NextOrderId;
        Order newOrder = item with { Id = id };
        DataSource.Orders.Add(newOrder);
    }

    public void Delete(int id)
    {
        if(Read(id) == null)
            throw new NotImplementedException();
        DataSource.Orders.Remove(Read(id));
    }

    public void DeleteAll()
    {
       DataSource.Orders.Clear(); ;
    }

    public Order? Read(int id)
    {
        foreach (Order itr in DataSource.Orders)
        {
            if (itr.Id == id)
                return itr;
        }
        return null; 
    }

    public List<Order> ReadAll()
    {
        List<Order> list = new List<Order>(DataSource.Orders);
        return list;
    }

    public void Update(Order item)
    {
        if(Read(item.Id) == null)
            throw new NotImplementedException();
        Delete(item.Id);
        DataSource.Orders.Add(item);
    }
}
