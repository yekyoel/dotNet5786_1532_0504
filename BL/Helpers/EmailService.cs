using System;
using System.Collections.Generic;
using System.Net.Mail;
using DalApi;

namespace Helpers;

/// <summary>
/// Very simple email helper used by BL to notify couriers.
/// In real deployments, replace pickup SMTP host/credentials with configuration.
/// </summary>
internal static class EmailService
{
    private static readonly IDal s_dal = Factory.Get;

    internal static void SendNewOrderNotification(DO.Order order)
    {
        try
        {
            var cfg = AdminManager.GetConfig();
            double storeLat = cfg?.Latitude ?? 0.0;
            double storeLon = cfg?.Longitude ?? 0.0;

            // Find couriers within max distance requirement
            var couriers = s_dal.Courier.ReadAll();
            var eligible = new List<DO.Courier>();

            foreach (var c in couriers)
            {
                if (!c.IsActive || string.IsNullOrWhiteSpace(c.Email))
                    continue;

                double maxDist = c.MaxDist ?? (cfg?.MaxDist ?? double.MaxValue);
                double distance = Tools.GetAerialDistanceKm(storeLat, storeLon, order.Latitude, order.Longitude);
                if (distance <= maxDist)
                    eligible.Add(c);
            }

            if (eligible.Count == 0)
                return; // nothing to notify

            foreach (var courier in eligible)
            {
                try
                {
                    using var msg = new MailMessage();
                    msg.To.Add(courier.Email);
                    msg.Subject = $"New order #{order.Id} available";
                    msg.Body =
                        $"Customer: {order.CustFullName}\n" +
                        $"Phone: {order.CusNum}\n" +
                        $"Address: {order.FullAdd}\n" +
                        $"Weight: {order.Weight} kg\n" +
                        $"Type: {order.Food}";

                    // NOTE: this assumes a local pickup directory or dev SMTP; adjust as needed.
                    using var client = new SmtpClient("localhost");
                    client.Send(msg);
                }
                catch
                {
                    // best-effort per courier
                }
            }
        }
        catch
        {
            // swallow – email failures must not break core flow
        }
    }
}
