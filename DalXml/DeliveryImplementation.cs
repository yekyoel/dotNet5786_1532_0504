namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;
using System.Xml.Linq;
internal class DeliveryImplementation : IDelivery
{
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

    public void Create(Delivery item)
    {
        XElement deliveriesRootElem = XMLTools.LoadListFromXMLElement(Config.s_deliveries_xml);
        int id = Config.NextDeliveryId;
        Delivery newDelivery = item with { Id = id };
        deliveriesRootElem.Add(createDeliveryElement(newDelivery));
        XMLTools.SaveListToXMLElement(deliveriesRootElem, Config.s_deliveries_xml);
    }

    public void Delete(int id)
    {
        XElement deliveriesRootElem = XMLTools.LoadListFromXMLElement(Config.s_deliveries_xml);
        XElement? elem = deliveriesRootElem.Elements().FirstOrDefault(st => (int?)st.Element("Id") == id);
        if (elem is null)
            throw new DO.DalDoesNotExistException($"Delivery with ID={id} does Not exist");
        elem.Remove();
        XMLTools.SaveListToXMLElement(deliveriesRootElem, Config.s_deliveries_xml);
    }

    public void DeleteAll()
    {
        XMLTools.SaveListToXMLElement(new XElement(Config.s_deliveries_xml), Config.s_deliveries_xml);
    }

    public Delivery? Read(int id)
    {
        XElement? deliveryElem =
    XMLTools.LoadListFromXMLElement(Config.s_deliveries_xml).Elements().FirstOrDefault(st => (int?)st.Element("Id") == id);
        return deliveryElem is null ? null : getDelivery(deliveryElem);
    }

    public Delivery? Read(Func<Delivery, bool> filter)
    {
        return XMLTools.LoadListFromXMLElement(Config.s_deliveries_xml).Elements().Select(s => getDelivery(s)).FirstOrDefault(filter);
    }

    public IEnumerable<Delivery> ReadAll(Func<Delivery, bool>? filter = null)
    {
        var items = XMLTools.LoadListFromXMLElement(Config.s_deliveries_xml).Elements().Select(s => getDelivery(s));
        return filter == null ? items : items.Where(filter);
    }

    public void Update(Delivery item)
    {
        XElement deliveriesRootElem = XMLTools.LoadListFromXMLElement(Config.s_deliveries_xml);

        (deliveriesRootElem.Elements().FirstOrDefault(st => (int?)st.Element("Id") == item.Id)
        ?? throw new DO.DalDoesNotExistException($"Delivery with ID={item.Id} does Not exist"))
                .Remove();

       deliveriesRootElem.Add(createDeliveryElement(item));

        XMLTools.SaveListToXMLElement(deliveriesRootElem, Config.s_deliveries_xml);
    }

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

