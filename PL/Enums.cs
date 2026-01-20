using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PL
{

    /// <summary>
    /// Provides an enumerable collection of all values defined in the BO.ShippingMethod enumeration.
    /// </summary>
    internal class VehicleTypeCollections : IEnumerable
    {
        static readonly IEnumerable<BO.ShippingMethod> s_enums =
        (Enum.GetValues(typeof(BO.ShippingMethod)) as IEnumerable<BO.ShippingMethod>)!;

        public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
    }

    /// <summary>
    /// Provides an enumerable collection of all values defined in the OrderStatus enumeration.
    /// </summary>
    internal class OrderStatusCollections : IEnumerable
    {
        static readonly IEnumerable<BO.OrderStatus> s_enums =
        (Enum.GetValues(typeof(BO.OrderStatus)) as IEnumerable<BO.OrderStatus>)!;

        public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
    }

    /// <summary>
    /// Provides an enumerable collection of all values defined in the OrderType enumeration.
    /// </summary>
    /// <remarks>This class enables iteration over the OrderType values using standard collection iteration
    /// patterns. It is intended for internal use within the assembly.</remarks>
    internal class OrderTypeCollections : IEnumerable
    {
        static readonly IEnumerable<BO.OrderType> s_enums =
        (Enum.GetValues(typeof(BO.OrderType)) as IEnumerable<BO.OrderType>)!;
        public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
    }
}

