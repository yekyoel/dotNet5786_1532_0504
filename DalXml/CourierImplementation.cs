namespace Dal;
using DalApi;
using DO;

internal class CourierImplementation : ICourier
{
    // create a new courier
    public void Create(Courier item)
    {
        List<Courier> couriers = XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);
        int id = Config.NextOrderId; // assign a new id consistently like OrderImplementation
        Courier newCourier = item with { Id = id };
        if (couriers.Exists(c => c.Id == newCourier.Id))
            throw new DalAlreadyExistExceptions($"Courier with ID={newCourier.Id} already exists");
        couriers.Add(newCourier);
        XMLTools.SaveListToXMLSerializer(couriers, Config.s_couriers_xml);
    }

    // delete courier by ID
    public void Delete(int id)
    {
        List<Courier> couriers = XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);
        if (couriers.RemoveAll(it => it.Id == id) == 0)
            throw new DalDoesNotExistException($"Courier with ID={id} does Not exist");
        XMLTools.SaveListToXMLSerializer(couriers, Config.s_couriers_xml);
    }

    // delete all couriers
    public void DeleteAll()
    {
        XMLTools.SaveListToXMLSerializer(new List<Courier>(), Config.s_couriers_xml);
    }

    // read courier by ID
    public Courier? Read(int id)
    {
        List<Courier> couriers = XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);
        return couriers.Find(c => c.Id == id);
    }

    // read courier by filter
    public Courier? Read(Func<Courier, bool> filter)
    {
        if (filter is null) return null;
        List<Courier> couriers = XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);
        return couriers.Find(new Predicate<Courier>(filter));
    }

    public IEnumerable<Courier> ReadAll(Func<Courier, bool>? filter = null)
    {
        List<Courier> couriers = XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);
        return filter == null ? couriers : couriers.FindAll(c => filter(c));
    }

    public void Update(Courier item)
    {
        List<Courier> couriers = XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);
        if (couriers.RemoveAll(it => it.Id == item.Id) == 0)
            throw new DalDoesNotExistException($"Courier with ID={item.Id} does Not exist");
        couriers.Add(item);
        XMLTools.SaveListToXMLSerializer(couriers, Config.s_couriers_xml);
    }
}
