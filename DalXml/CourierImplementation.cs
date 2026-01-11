namespace Dal;
using DalApi;
using DO;
using System.Runtime.CompilerServices;


/// <summary>
/// class Courier Implementation that implements the ICourier interface for managing Courier data in XML format.
/// </summary>
internal class CourierImplementation : ICourier
{
    // create a new courier
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Create(Courier item)
    {
        List<Courier> couriers = XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);
        Courier newCourier = item;
        if (couriers.Exists(c => c.Id == newCourier.Id))
            throw new DalAlreadyExistExceptions($"Courier with ID={newCourier.Id} already exists");
        couriers.Add(newCourier);
        XMLTools.SaveListToXMLSerializer(couriers, Config.s_couriers_xml);
    }

    // delete courier by id
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Delete(int id)
    {
        List<Courier> couriers = XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);
        if (couriers.RemoveAll(it => it.Id == id) == 0)
            throw new DalDoesNotExistException($"Courier with ID={id} does Not exist");
        XMLTools.SaveListToXMLSerializer(couriers, Config.s_couriers_xml);
    }

    // delete all couriers
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void DeleteAll()
    {
        XMLTools.SaveListToXMLSerializer(new List<Courier>(), Config.s_couriers_xml);
    }

    // read courier by ID
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Courier? Read(int id)
    {
        List<Courier> couriers = XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);
        return couriers.Find(c => c.Id == id);
    }

    // read courier by filter
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Courier? Read(Func<Courier, bool> filter)
    {
        if (filter is null) return null;
        List<Courier> couriers = XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);
        return couriers.Find(new Predicate<Courier>(filter));
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerable<Courier> ReadAll(Func<Courier, bool>? filter = null)
    {
        List<Courier> couriers = XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);
        return filter == null ? couriers : couriers.FindAll(c => filter(c));
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Update(Courier item)
    {
        List<Courier> couriers = XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);
        if (couriers.RemoveAll(it => it.Id == item.Id) == 0)
            throw new DalDoesNotExistException($"Courier with ID={item.Id} does Not exist");
        couriers.Add(item);
        XMLTools.SaveListToXMLSerializer(couriers, Config.s_couriers_xml);
    }
}
