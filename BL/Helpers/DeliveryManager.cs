
using DalApi;

namespace Helpers;

internal static class DeliveryManager
{
    private static IDal s_dal = Factory.Get; //stage 4

    internal static DO.CompletionType? checkForSatus(DO.Order order)
    { 
        var status = s_dal?.Delivery?.Read(order.Id)?.End;
        return status;
    }
 
}
