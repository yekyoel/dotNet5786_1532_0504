namespace Dal;

using DalApi;
using DO;
using System.Collections.Generic;

/// <summary>
/// Provides an implementation of the Order interface for managing order entities.
/// </summary>
internal class OrderImplementation : IOrder
{
    /// <summary>
    /// Creates a new order and adds it to the data source.
    /// </summary>
    /// <param name="item"></param>
    public void Create(Order item)
    {
        int id = Config.NextOrderId;
        Order newOrder = item with { Id = id };
        DataSource.Orders.Add(newOrder);
    }

    /// <summary>
    /// Deletes an order by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <exception cref="DalIsUnchangeableExceptions"></exception>
    public void Delete(int id)
    {
        if(Read(id) == null)
            throw new DalIsUnchangeableExceptions("This is an ID you can't change");
        DataSource.Orders.Remove(Read(id));
    }

    /// <summary>
    /// Deletes all orders from the data source.
    /// </summary>
    public void DeleteAll()
    {
       DataSource.Orders.Clear(); ;
    }

    /// <summary>
    /// Reads an order by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Order? Read(int id)
    {
        return DataSource.Orders.FirstOrDefault(item => item.Id == id); //stage 2
    }

    /// <summary>
    /// Reads all orders, optionally filtered by a provided predicate.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public IEnumerable<Order> ReadAll(Func<Order, bool>? filter = null) //stage 2
       => filter == null
           ? DataSource.Orders.Select(item => item) : DataSource.Orders.Where(filter);

    /// <summary>
    /// Updates an existing order.
    /// </summary>
    /// <param name="item"></param>
    /// <exception cref="DalDoesNotExistException"></exception>
    public void Update(Order item)
    {
        if(Read(item.Id) == null)
            throw new DalDoesNotExistException($"Order with ID={item.Id} doesn't exist"); ;
        Delete(item.Id);
        DataSource.Orders.Add(item);
    }

    /// <summary>
    /// Reads an order that matches the provided filter.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public Order? Read(Func<Order, bool> filter)
    {
        // if no Order matches the filter, FirstOrDefault returns null
        return DataSource.Orders.FirstOrDefault(filter); 
    }
}
