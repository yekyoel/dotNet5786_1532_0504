using DalApi;
using DO;
using System.Linq;

namespace Helpers;

internal static class DeliveryManager
{
    private static IDal s_dal = Factory.Get; //stage 4
    internal static ObserverManager Observers = new(); // stage 5 not sure we need it here
    private static readonly AsyncMutex s_periodicMutex = new(); //stage 7

    /// <summary>
    /// Retrieves the first delivery associated with the specified order ID, if one exists.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order for which to retrieve the delivery.</param>
    /// <returns>A <see cref="DO.Delivery"/> object representing the delivery for the specified order, or <see langword="null"/>
    /// if no delivery is found.</returns>
    internal static DO.Delivery? GetDeliveryByOrderId(int orderId)
    {
        // Return the newest delivery for this order (by Id).
        // This prevents "random" older records being returned when multiple deliveries exist.
        IEnumerable<DO.Delivery?> deliveries;
        lock (AdminManager.BlMutex)
            deliveries = s_dal.Delivery.ReadAll(); // Get list under lock

        return deliveries
            .Where(d => d.OrderId == orderId)
            .OrderByDescending(d => d.Id)
            .FirstOrDefault();
    }

    /// <summary>
    /// Retrieves all deliveries from the data source.
    /// </summary>
    /// <returns></returns>
    internal static IEnumerable<DO.Delivery?> GetAllDeliveries()
    {
        lock (AdminManager.BlMutex)
            return s_dal.Delivery.ReadAll().ToList();
    }

    /// <summary>
    /// Periodic updates for deliveries:
    /// - Marks assigned, in-progress deliveries as Failed if they exceeded expected + risk thresholds.
    /// - Lightweight and resilient: per-delivery exceptions are swallowed.
    /// </summary>
    internal static void PeriodicDeliveriesUpdates(DateTime oldClock, DateTime newClock)
    {
        if (s_periodicMutex.CheckAndSetInProgress())
             return;
        try
        {
            if (newClock <= oldClock)
                return;

            var config = AdminManager.GetConfig();
            if (config == null)
                return;

            List<DO.Delivery?> deliveries;
            lock (AdminManager.BlMutex)
                deliveries = s_dal.Delivery.ReadAll().ToList();

            var updatedDeliveries = new List<DO.Delivery>();

            foreach (var d in deliveries)
            {
                if (d == null) continue;
                try
                {
                    // skip already finished deliveries
                    if (d.DeliveryEndTime.HasValue)
                        continue;

                    // only consider assigned deliveries (conservative)
                    if (d.ShippingMethod is null)
                        continue;

                    // determine reference start: delivery start, or order ordering time, or config clock
                    DO.Order? order;
                    lock(AdminManager.BlMutex)
                        order = s_dal.Order.Read(d.OrderId);

                    DateTime referenceStart = d.DeliveryStartTime
                                              ?? order?.StartTimeForOrdering
                                              ?? config.Clock;

                    DateTime expectedDeliveryTime = referenceStart.Add(config.MaxDelTime);
                    DateTime failThreshold = expectedDeliveryTime.Add(config.RiskRange);

                    if (newClock > failThreshold)
                    {
                        var updated = d with
                        {
                            End = DO.CompletionType.Failed,
                            DeliveryEndTime = newClock
                        };
                        lock (AdminManager.BlMutex)
                            s_dal.Delivery.Update(updated);

                        updatedDeliveries.Add(updated);
                    }
                }
                catch
                {
                    // swallow per-delivery failures and continue
                }
            }

            foreach (var updated in updatedDeliveries)
            {
                 // notify observers: delivery, related order, and courier
                 Observers.NotifyItemUpdated(updated.Id);
                 OrderManager.Observers.NotifyItemUpdated(updated.OrderId);
                 if (updated.CourierId > 0)
                      CourierManager.Observers.NotifyItemUpdated(updated.CourierId);
            }
            if (updatedDeliveries.Count > 0)
            {
                 Observers.NotifyListUpdated();
                 OrderManager.Observers.NotifyListUpdated();
            }
        }
        catch
        {
            // swallow outer exceptions to avoid breaking clock update caller
        }
        finally
        {
             s_periodicMutex.UnsetInProgress();
        }

    }

    /// <summary>
    /// Marks the specified delivery as completed and updates its status to delivered.
    /// </summary>
    /// <param name="deliveryId">The unique identifier of the delivery to complete.</param>
    /// <exception cref="KeyNotFoundException">Thrown if a delivery with the specified deliveryId does not exist.</exception>
    internal static void CompleteDelivery(int deliveryId)
    {
        DO.Delivery? delivery;
        lock(AdminManager.BlMutex)
            delivery = s_dal.Delivery.Read(deliveryId);

        if (delivery == null)
            throw new KeyNotFoundException($"Delivery with ID {deliveryId} not found");
        var updated = delivery with 
        {
            End = DO.CompletionType.Delivered,
            DeliveryEndTime = DateTime.Now
        };
        lock (AdminManager.BlMutex)
            s_dal.Delivery.Update(updated);

        // notify observers: delivery, related order, and courier
        Observers.NotifyItemUpdated(updated.Id);
        Observers.NotifyListUpdated();
        OrderManager.Observers.NotifyItemUpdated(updated.OrderId);
        OrderManager.Observers.NotifyListUpdated();
        if (updated.CourierId > 0)
            CourierManager.Observers.NotifyItemUpdated(updated.CourierId);
    }

    internal static void CompleteDelivery(int deliveryId, DO.CompletionType completionType)
    {
        DO.Delivery? delivery;
        lock (AdminManager.BlMutex)
            delivery = s_dal.Delivery.Read(deliveryId);

        if (delivery == null)
            throw new KeyNotFoundException($"Delivery with ID {deliveryId} not found");
        var updated = delivery with
        {
            End = completionType,
            DeliveryEndTime = DateTime.Now
        };
        lock (AdminManager.BlMutex)
            s_dal.Delivery.Update(updated);

        // notify observers: delivery, related order, and courier
        Observers.NotifyItemUpdated(updated.Id);
        Observers.NotifyListUpdated();
        OrderManager.Observers.NotifyItemUpdated(updated.OrderId);
        OrderManager.Observers.NotifyListUpdated();
        if (updated.CourierId > 0)
            CourierManager.Observers.NotifyItemUpdated(updated.CourierId);
    }

}
