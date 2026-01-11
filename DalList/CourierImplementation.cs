namespace Dal;

using DalApi;
using DO;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

/// <summary>
/// Provides an implementation of the Courier interface for managing courier entities.
/// </summary>
/// <remarks>This class offers methods to create, read, update, and delete courier entities, as well as retrieve
/// collections of couriers. It ensures that operations adhere to constraints such as unique IDs for couriers and
/// prevents actions on non-existent entities.</remarks>

internal class CourierImplementation : ICourier
{
    /// <summary>
    ///  Creates a new courier item in the data source.
    /// </summary>
    /// <param name="item"></param>
    /// <exception cref="DalAlreadyExistExceptions"></exception>
    [MethodImpl(MethodImplOptions.Synchronized)]

    public void Create(Courier item)
    {
        if(Read(item.Id) != null)
            throw new DalAlreadyExistExceptions($"Courier with ID={item.Id} already exists"); // throwing an exception of one of the exception classes we created
        DataSource.Couriers.Add(item);
    }

    /// <summary>
    /// Deletes a courier item from the data source by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <exception cref="DalDoesNotExistException"></exception>
    [MethodImpl(MethodImplOptions.Synchronized)]

    public void Delete(int id)
    {
        if (Read(id) == null)
            throw new DalDoesNotExistException($"Courier with ID={id} doesn't exist"); // // throwing an exception of one of the exception classes we created
        DataSource.Couriers.Remove(Read(id)!);// read cant be null here because of the check above
    }

    /// <summary>
    /// Deletes all courier items from the data source.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]

    public void DeleteAll()
    {
        foreach (Courier itr in DataSource.Couriers.ToArray()) // to avoid modifying collection during iteration
        {
            DataSource.Couriers.Remove(itr); // remove each courier
        }
    }

    /// <summary>
    /// Reads a courier item from the data source by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Courier? Read(int id)
    {
        return DataSource.Couriers.FirstOrDefault(item => item.Id == id); 
    }

    /// <summary>
    /// Reads all courier items from the data source, optionally filtered by a provided predicate.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerable<Courier> ReadAll(Func<Courier, bool>? filter = null) 
        => filter == null
            ? DataSource.Couriers.Select(item => item) : DataSource.Couriers.Where(filter);

    /// <summary>
    /// Updates an existing courier item in the data source.
    /// </summary>
    /// <param name="item"></param>
    /// <exception cref="DalDoesNotExistException"></exception>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Update(Courier item)
    {
        if (Read(item.Id) == null)
            throw new DalDoesNotExistException($"Courier with ID={item.Id} doesn't exist"); // cannot update non-existing object
        Delete(item.Id);
        DataSource.Couriers.Add(item);
    }

    /// <summary>
    /// Reads a courier item from the data source that matches the provided filter predicate.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Courier? Read(Func<Courier, bool> filter)
    {
        // if no courier matches the filter, FirstOrDefault returns null
        return DataSource.Couriers.FirstOrDefault(filter); 

    }
}