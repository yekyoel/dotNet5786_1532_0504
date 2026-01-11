namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

/// <summary>
/// class Delivery Implementation that implements the IDelivery interface for managing Delivery data in XML format.
/// </summary>
internal class DeliveryImplementation : IDelivery
{
    // Converts an XElement to a Delivery object
    [MethodImpl(MethodImplOptions.Synchronized)]
    static Delivery getDelivery(XElement d)
    {
        return new DO.Delivery()
        {
            Id = d.ToIntNullable("Id") ?? throw new FormatException("can't convert id"),
            OrderId = d.ToIntNullable("OrderId") ?? 0,
            CourierId = d.ToIntNullable("CourierId") ?? 0,
            ShippingMethod = d.ToEnumNullable<ShippingMethod>("ShippingMethod"),
            DeliveryStartTime = d.ToDateTimeNullable("DeliveryStartTime"),
            Distance = d.ToDoubleNullable("Distance"),
            End = d.ToEnumNullable<CompletionType>("End"),
            DeliveryEndTime = d.ToDateTimeNullable("DeliveryEndTime")
        };
    }

    // Creates a new Delivery entry in the XML data store
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Create(Delivery item)
    {
        XElement deliveriesRootElem = XMLTools.LoadListFromXMLElement(Config.s_deliveries_xml);
        int id = Config.NextDeliveryId;
        Delivery newDelivery = item with { Id = id };
        deliveriesRootElem.Add(createDeliveryElement(newDelivery));
        XMLTools.SaveListToXMLElement(deliveriesRootElem, Config.s_deliveries_xml);
    }

    // Deletes a Delivery entry by its ID
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Delete(int id)
    {
        XElement deliveriesRootElem = XMLTools.LoadListFromXMLElement(Config.s_deliveries_xml);
        XElement? elem = deliveriesRootElem.Elements().FirstOrDefault(st => (int?)st.Element("Id") == id);
        if (elem is null)
            throw new DO.DalDoesNotExistException($"Delivery with ID={id} does Not exist");
        elem.Remove();
        XMLTools.SaveListToXMLElement(deliveriesRootElem, Config.s_deliveries_xml);
    }

    // Deletes all Delivery entries
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void DeleteAll()
    {
        XMLTools.SaveListToXMLElement(new XElement(Config.s_deliveries_xml), Config.s_deliveries_xml);
    }

    // Reads a Delivery entry by its ID
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Delivery? Read(int id)
    {
        XElement? deliveryElem =
    XMLTools.LoadListFromXMLElement(Config.s_deliveries_xml).Elements().FirstOrDefault(st => (int?)st.Element("Id") == id);
        return deliveryElem is null ? null : getDelivery(deliveryElem);
    }

    // Reads a Delivery entry that matches a given filter
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Delivery? Read(Func<Delivery, bool> filter)
    {
        return XMLTools.LoadListFromXMLElement(Config.s_deliveries_xml).Elements().Select(s => getDelivery(s)).FirstOrDefault(filter);
    }

    // Reads all Delivery entries, optionally filtered by a given predicate
    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerable<Delivery> ReadAll(Func<Delivery, bool>? filter = null)
    {
        var items = XMLTools.LoadListFromXMLElement(Config.s_deliveries_xml).Elements().Select(s => getDelivery(s));
        return filter == null ? items : items.Where(filter);
    }

    // Updates an existing Delivery entry
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Update(Delivery item)
    {
        XElement deliveriesRootElem = XMLTools.LoadListFromXMLElement(Config.s_deliveries_xml);

        (deliveriesRootElem.Elements().FirstOrDefault(st => (int?)st.Element("Id") == item.Id)
        ?? throw new DO.DalDoesNotExistException($"Delivery with ID={item.Id} does Not exist"))
                .Remove();

       deliveriesRootElem.Add(createDeliveryElement(item));

        XMLTools.SaveListToXMLElement(deliveriesRootElem, Config.s_deliveries_xml);
    }

    // Helper method to create an XElement from a Delivery object
    [MethodImpl(MethodImplOptions.Synchronized)]
    private XElement createDeliveryElement(Delivery item)
    {
        return new XElement("Delivery",
            new XElement("Id", item.Id),
            new XElement("OrderId", item.OrderId),
            new XElement("CourierId", item.CourierId),
            new XElement("ShippingMethod", item.ShippingMethod),
            new XElement("DeliveryStartTime", item.DeliveryStartTime),
            new XElement("Distance", item.Distance),
            new XElement("End", item.End),
            new XElement("DeliveryEndTime", item.DeliveryEndTime)
        );
    }
}

