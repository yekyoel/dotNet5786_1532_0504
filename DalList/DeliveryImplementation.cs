namespace Dal;

using DalApi;
using DO;
using System.Collections.Generic;

/// <summary>
/// Provides an implementation of the Delivery interface for managing delivery entities.
/// </summary>
internal class DeliveryImplementation : IDelivery
{
    /// <summary>
    /// Creates a new delivery entity.
    /// </summary>
    /// <param name="item"></param>
    public void Create(Delivery item)
    {
        int id1 = Config.NextOrderId;
        int id2 = Config.NextDeliveryId;
        Delivery newDel = item with { Id = id1, OrderId = id2 };
        DataSource.Deliveries.Add(newDel);
    }

    /// <summary>
    /// Deletes a delivery entity by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <exception cref="DalDoesNotExistException"></exception>
    public void Delete(int id)
    {
        if (Read(id) == null)
            throw new DalDoesNotExistException($"Delivery with ID={id} doesn't exist"); // throw exception from Exceptions.cs
        DataSource.Deliveries.Remove(Read(id)); 
    }

    /// <summary>
    /// Deletes all delivery entities.
    /// </summary>
    public void DeleteAll()
    {
            DataSource.Deliveries.Clear();
    }

    /// <summary>
    /// Reads a delivery entity by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Delivery? Read(int id)
    {
        return DataSource.Deliveries.FirstOrDefault(item => item.Id == id); //stage 2
    }

    /// <summary>
    /// Reads all delivery entities, optionally filtered by a provided function.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public IEnumerable<Delivery> ReadAll(Func<Delivery, bool>? filter = null) //stage 2
       => filter == null
           ? DataSource.Deliveries.Select(item => item) : DataSource.Deliveries.Where(filter);

    /// <summary>
    /// Updates an existing delivery entity.
    /// </summary>
    /// <param name="item"></param>
    /// <exception cref="DalAlreadyExistExceptions"></exception>
    public void Update(Delivery item)
    {
        if (Read(item.Id) == null)
            throw new DalAlreadyExistExceptions($"Delivery with ID={item.Id} already exists"); // throw exception from Exceptions.cs
        Delete(item.Id);
        DataSource.Deliveries.Add(item); // add the updated item
    }

    /// <summary>
    /// Reads a delivery entity that matches the provided filter function.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public Delivery? Read(Func<Delivery, bool> filter)
    {
        // if no delivery matches the filter, FirstOrDefault returns null
        return DataSource.Deliveries.FirstOrDefault(filter); //stage 2

    }


}