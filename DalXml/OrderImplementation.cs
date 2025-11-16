namespace Dal;
using DalApi;
using DO;

internal class OrderImplementation : IOrder
{
    public void Create(Order item)
    {
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);
        int id = Config.NextOrderId;
        Order newOrder = item with { Id = id };
        orders.Add(newOrder);
        XMLTools.SaveListToXMLSerializer(orders, Config.s_orders_xml);
    }

    public void Delete(int id)
    {
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);
        if (orders.RemoveAll(it => it.Id == id) == 0)
            throw new DalDoesNotExistException($"Order with ID={id} does Not exist");
        XMLTools.SaveListToXMLSerializer(orders, Config.s_orders_xml);
    }

    public void DeleteAll()
    {
        XMLTools.SaveListToXMLSerializer(new List<Order>(), Config.s_orders_xml); // delete all orders by saving empty list
    }

    public Order? Read(int id)
    {
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);
        return orders.Find(o => o.Id == id);
    }

    public Order? Read(Func<Order, bool> filter)
    {
        if (filter is null) return null;
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);
        return orders.Find(new System.Predicate<Order>(filter));
    }

    public IEnumerable<Order> ReadAll(Func<Order, bool>? filter = null)
    {
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);
        return filter == null ? orders : orders.FindAll(o => filter(o));
    }

    public void Update(Order item)
    {
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);
        if (orders.RemoveAll(it => it.Id == item.Id) == 0)
            throw new DalDoesNotExistException($"Order with ID={item.Id} does Not exist");
        orders.Add(item);
        XMLTools.SaveListToXMLSerializer(orders, Config.s_orders_xml);
    }
}

