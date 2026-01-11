namespace Dal;
using DalApi;
using DO;
using System.Runtime.CompilerServices;

/// <summary>
/// class Order Implementation that implements the IOrder interface for managing Order data in XML format.
/// </summary>
internal class OrderImplementation : IOrder
{
    // Create a new Order and save it to the XML file.
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Create(Order item)
    {
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);
        int id = Config.NextOrderId;
        Order newOrder = item with { Id = id };
        orders.Add(newOrder);
        XMLTools.SaveListToXMLSerializer(orders, Config.s_orders_xml);
    }

    // Delete an Order by its ID from the XML file.
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Delete(int id)
    {
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);
        if (orders.RemoveAll(it => it.Id == id) == 0)
            throw new DalDoesNotExistException($"Order with ID={id} does Not exist");
        XMLTools.SaveListToXMLSerializer(orders, Config.s_orders_xml);
    }

    // Delete all Orders from the XML file.
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void DeleteAll()
    {
        XMLTools.SaveListToXMLSerializer(new List<Order>(), Config.s_orders_xml); // delete all orders by saving empty list
    }

    // Read an Order by its ID from the XML file.
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Order? Read(int id)
    {
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);
        return orders.Find(o => o.Id == id);
    }

    // Read an Order that matches the given filter from the XML file.
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Order? Read(Func<Order, bool> filter)
    {
        if (filter is null) return null;
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);
        return orders.Find(new System.Predicate<Order>(filter));
    }

    // Read all Orders, optionally filtered by the given predicate, from the XML file.
    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerable<Order> ReadAll(Func<Order, bool>? filter = null)
    {
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);
        return filter == null ? orders : orders.FindAll(o => filter(o));
    }

    // Update an existing Order in the XML file.
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Update(Order item)
    {
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);
        if (orders.RemoveAll(it => it.Id == item.Id) == 0)
            throw new DalDoesNotExistException($"Order with ID={item.Id} does Not exist");
        orders.Add(item);
        XMLTools.SaveListToXMLSerializer(orders, Config.s_orders_xml);
    }
}

