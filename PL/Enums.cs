using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PL
{

    internal class VehicleTypeCollections : IEnumerable
    {
        static readonly IEnumerable<BO.ShippingMethod> s_enums =
        (Enum.GetValues(typeof(BO.ShippingMethod)) as IEnumerable<BO.ShippingMethod>)!;

        public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
    }

    internal class OrderStatusCollections : IEnumerable
    {
        static readonly IEnumerable<BO.OrderStatus> s_enums =
        (Enum.GetValues(typeof(BO.OrderStatus)) as IEnumerable<BO.OrderStatus>)!;

        public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
    }

    internal class OrderTypeCollections : IEnumerable
    {
        static readonly IEnumerable<BO.OrderType> s_enums =
        (Enum.GetValues(typeof(BO.OrderType)) as IEnumerable<BO.OrderType>)!;
        public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
    }
}

