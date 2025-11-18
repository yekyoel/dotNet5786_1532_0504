namespace Dal;
using DalApi;
using DO;

internal class DeliveryImplementation : IDelivery
{
    // Create a new delivery
    public void Create(Delivery item)
    {
        List<Delivery> deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.s_deliveries_xml);
        int id = Config.NextDeliveryId;
        Delivery newDel = item with { Id = id }; // assign new ID
        if (deliveries.Exists(d => d.Id == newDel.Id))
            throw new DalAlreadyExistExceptions($"Delivery with ID={newDel.Id} already exists");
        deliveries.Add(newDel);
        XMLTools.SaveListToXMLSerializer(deliveries, Config.s_deliveries_xml);
    }

    // Delete a delivery by ID
    public void Delete(int id)
    {
        List<Delivery> deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.s_deliveries_xml);
        if (deliveries.RemoveAll(it => it.Id == id) == 0)
            throw new DalDoesNotExistException($"Delivery with ID={id} does Not exist");
        XMLTools.SaveListToXMLSerializer(deliveries, Config.s_deliveries_xml);
    }

    // Delete all deliveries
    public void DeleteAll()
    {
        XMLTools.SaveListToXMLSerializer(new List<Delivery>(), Config.s_deliveries_xml); // delete all by saving empty list
    }

    // Read a delivery by ID
    public Delivery? Read(int id)
    {
        List<Delivery> deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.s_deliveries_xml);
        return deliveries.Find(d => d.Id == id);
    }

    // Read a delivery by filter
    public Delivery? Read(Func<Delivery, bool> filter)
    {
        if (filter is null) return null;
        List<Delivery> deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.s_deliveries_xml);
        return deliveries.Find(new System.Predicate<Delivery>(filter));
    }

    // Read all deliveries with optional filter
    public IEnumerable<Delivery> ReadAll(Func<Delivery, bool>? filter = null)
    {
        List<Delivery> deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.s_deliveries_xml);
        return filter == null ? deliveries : deliveries.FindAll(d => filter(d));
    }

    // Update a delivery
    public void Update(Delivery item)
    {
        List<Delivery> deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.s_deliveries_xml);
        if (deliveries.RemoveAll(it => it.Id == item.Id) == 0)
            throw new DalDoesNotExistException($"Delivery with ID={item.Id} does Not exist");
        deliveries.Add(item);
        XMLTools.SaveListToXMLSerializer(deliveries, Config.s_deliveries_xml);
    }
}

