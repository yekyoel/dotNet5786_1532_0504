using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DalApi;

public interface IDal
{

    ICourier Courier { get; }
    IOrder Order { get; }
    IDelivery Delivery { get; }

    IConfig Config { get; }
    void ResetDB();



}
