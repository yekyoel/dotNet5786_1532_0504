using DalApi;
using DO;
using System.Linq;

namespace Helpers;

internal static class DeliveryManager
{
    private static IDal s_dal = Factory.Get; //stage 4
    internal static ObserverManager Observers = new(); // stage 5 not sure we need it here

    /// <summary>
    /// Retrieves the first delivery associated with the specified order ID, if one exists.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order for which to retrieve the delivery.</param>
    /// <returns>A <see cref="DO.Delivery"/> object representing the delivery for the specified order, or <see langword="null"/>
    /// if no delivery is found.</returns>
    internal static DO.Delivery? GetDeliveryByOrderId(int orderId)
    {
        // Return the first delivery that references the given order id (or null)
        return s_dal.Delivery.ReadAll().FirstOrDefault(d => d.OrderId == orderId);
    }

    /// <summary>
    /// Retrieves all deliveries from the data source.
    /// </summary>
    /// <returns></returns>
    internal static IEnumerable<DO.Delivery?> GetAllDeliveries()
    {
        return s_dal.Delivery.ReadAll().ToList();
    }

    /// <summary>
    /// Periodic updates for deliveries:
    /// - Marks assigned, in-progress deliveries as Failed if they exceeded expected + risk thresholds.
    /// - Lightweight and resilient: per-delivery exceptions are swallowed.
    /// </summary>
    internal static void PeriodicDeliveriesUpdates(DateTime oldClock, DateTime newClock)
    {
        try
        {
            if (newClock <= oldClock)
                return;

            var config = AdminManager.GetConfig();
            if (config == null)
                return;

            var deliveries = s_dal.Delivery.ReadAll();

            foreach (var d in deliveries)
            {
                try
                {
                    // skip already finished deliveries
                    if (d.DeliveryEndTime.HasValue)
                        continue;

                    // only consider assigned deliveries (conservative)
                    if (d.ShippingMethod is null)
                        continue;

                    // determine reference start: delivery start, or order ordering time, or config clock
                    var order = s_dal.Order.Read(d.OrderId);
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
                        s_dal.Delivery.Update(updated);
                    }
                }
                catch
                {
                    // swallow per-delivery failures and continue
                }
            }
        }
        catch
        {
            // swallow outer exceptions to avoid breaking clock update caller
        }

    }

    /// <summary>
    /// Marks the specified delivery as completed and updates its status to delivered.
    /// </summary>
    /// <param name="deliveryId">The unique identifier of the delivery to complete.</param>
    /// <exception cref="KeyNotFoundException">Thrown if a delivery with the specified deliveryId does not exist.</exception>
    internal static void CompleteDelivery(int deliveryId)
    {
        var delivery = s_dal.Delivery.Read(deliveryId);
        if (delivery == null)
            throw new KeyNotFoundException($"Delivery with ID {deliveryId} not found");
        var updated = delivery with
        {
            End = DO.CompletionType.Delivered,
            DeliveryEndTime = DateTime.Now
        };
        s_dal.Delivery.Update(updated);
    }

}
