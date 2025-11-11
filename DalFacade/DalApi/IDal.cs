using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DalApi;

/// <summary>
/// Represents a data access layer (DAL) interface that provides access to core services  and operations related to
/// couriers, orders, deliveries, and configuration.`
/// </summary>
/// <remarks>This interface defines the contract for accessing and managing data entities and  operations in the
/// system. It includes properties for accessing specific services  and a method for resetting the database
/// state.</remarks>
public interface IDal
{

    ICourier Courier { get; }
    IOrder Order { get; }
    IDelivery Delivery { get; }

    IConfig Config { get; }
    void ResetDB();



}
