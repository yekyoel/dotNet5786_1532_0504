using DalApi;

namespace Helpers;

internal static class DeliveryManager
{
    private static IDal s_dal = Factory.Get; //stage 4

    internal static DO.Delivery? GetDeliveryByOrderId(int orderId)
    {
        return s_dal?.Delivery?.Read(orderId);
    }
}
