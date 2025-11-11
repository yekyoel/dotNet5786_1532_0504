using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DalApi;

/// <summary>
/// Defines a generic interface for performing basic Create, Read, Update, and Delete (CRUD) operations on entities of
/// </summary>
/// <remarks>This interface provides methods for managing entities in a data access layer (DAL). It supports
/// operations such as creating new entities, reading entities by ID or filter, updating existing entities, and deleting
/// entities individually or in bulk.</remarks>
/// <typeparam name="T">The type of the entity on which CRUD operations are performed. Must be a reference type.</typeparam>
public interface ICrud<T> where T : class
{
    void Create(T item); //Creates new entity object in DAL
    T? Read(int id); //Reads entity object by its ID 
    IEnumerable<T> ReadAll(Func<T, bool>? filter = null); // Reads all entity objects, optionally filtered by a predicate
    void Update(T item); //Updates entity object
    void Delete(int id); //Deletes an object by its Id
    void DeleteAll(); //Delete all entity objects
    T? Read(Func<T, bool> filter); //Reads a single entity object that matches the specified filter criteria
}

