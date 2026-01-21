using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using DalApi;

namespace Helpers;

internal static class EmailService
{
    private static readonly IDal s_dal = Factory.Get;
    // Configure these with your Gmail address and an App Password (not your normal password)
    // Create an App Password in Google Account > Security > App passwords.
    //private const string GmailUser = "yoelmoshey@gmail.com";
    //private const string GmailAppPassword = "ipmu nqqq risi qxvo";
    private const string GmailUser = "someone@gmail.com";
    private const string GmailAppPassword = "123 456 789";

    // Simple diagnostics: write failures to a temp log file
    private static readonly string s_logPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "EmailService.log");
    private static void Log(string text)
    {
        try
        {
            var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {text}";
            System.IO.File.AppendAllText(s_logPath, line + Environment.NewLine);
        }
        catch { }
    }

    // Returns number of successfully sent emails (0 means none were sent)
    internal static async Task<int> SendNewOrderNotificationAsync(DO.Order order)
    {
        var sentCount = 0;
        try
        {
            var cfg = AdminManager.GetConfig();
            double storeLat = cfg?.Latitude ?? 0.0;
            double storeLon = cfg?.Longitude ?? 0.0;

            // Find couriers within max distance requirement
            IEnumerable<DO.Courier?> couriers;
            lock (AdminManager.BlMutex)
                couriers = s_dal.Courier.ReadAll().ToList();

            var eligible = new List<DO.Courier>();

            foreach (var c in couriers)
            {
                if (c == null) continue;
                if (!c.IsActive || string.IsNullOrWhiteSpace(c.Email))
                    continue;

                double maxDist = c.MaxDist ?? (cfg?.MaxDist ?? double.MaxValue);
                double distance = Tools.GetAerialDistanceKm(storeLat, storeLon, order.Latitude, order.Longitude);
                if (distance <= maxDist)
                    eligible.Add(c);
            }

            if (eligible.Count == 0)
                return 0; // nothing to notify

            foreach (var courier in eligible)
            {
                try
                {
                    using var msg = new MailMessage();
                    // From must match the authenticated Gmail account
                    msg.From = new MailAddress(GmailUser);
                    msg.To.Add(courier.Email);
                    msg.Subject = $"New order #{order.Id} available";
                    msg.Body =
                        $"Customer: {order.CustFullName}\n" +
                        $"Phone: {order.CusNum}\n" +
                        $"Address: {order.FullAdd}\n" +
                        $"Weight: {order.Weight} kg\n" +
                        $"Type: {order.Food}";

                    using var client = new SmtpClient("smtp.gmail.com", 587)
                    {
                        EnableSsl = true,
                        Credentials = new System.Net.NetworkCredential(GmailUser, GmailAppPassword)
                    };
                    await client.SendMailAsync(msg);
                    sentCount++;
                }
                catch (Exception ex)
                {
                    Log($"Send failed to {courier.Email}: {ex.Message}");
                    // ignore per-recipient failure, continue to others
                }
            }
        }
        catch (Exception ex)
        {
            Log($"EmailService outer failure: {ex.Message}");
            // swallow – email failures must not break core flow
        }
        return sentCount;
    }

    // Notify a specific courier that an order has been assigned to them
    internal static async Task SendOrderAssignedToCourierAsync(DO.Order order, int courierId)
    {
        try
        {
            if (courierId <= 0)
                return;

            DO.Courier? courier;
            lock (AdminManager.BlMutex)
                courier = s_dal.Courier.Read(courierId);
            if (courier is null || string.IsNullOrWhiteSpace(courier.Email))
                return;

            using var msg = new MailMessage();
            msg.From = new MailAddress(GmailUser);
            msg.To.Add(courier.Email);
            msg.Subject = $"Order #{order.Id} assigned to you";
            msg.Body =
                $"You have been assigned order #{order.Id}.\n" +
                $"Customer: {order.CustFullName}\n" +
                $"Phone: {order.CusNum}\n" +
                $"Address: {order.FullAdd}\n" +
                $"Weight: {order.Weight} kg\n" +
                $"Type: {order.Food}";

            using var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new System.Net.NetworkCredential(GmailUser, GmailAppPassword)
            };
            await client.SendMailAsync(msg);
        }
        catch (Exception ex)
        {
            // optional diag
        }
    }

    // Notify a courier that an order in progress was cancelled
    internal static async Task SendDeliveryCancelledNotificationAsync(DO.Order order, int courierId)
    {
        try
        {
            if (courierId <= 0)
                return;

            DO.Courier? courier;
            lock (AdminManager.BlMutex)
                courier = s_dal.Courier.Read(courierId);
            if (courier is null || string.IsNullOrWhiteSpace(courier.Email))
                return;

            using var msg = new MailMessage();
            msg.From = new MailAddress(GmailUser);
            msg.To.Add(courier.Email);
            msg.Subject = $"Order #{order.Id} has been cancelled";
            msg.Body =
                $"The order you were handling (#{order.Id}) was cancelled.\n" +
                $"Customer: {order.CustFullName}\n" +
                $"Phone: {order.CusNum}\n" +
                $"Address: {order.FullAdd}";

            using var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new System.Net.NetworkCredential(GmailUser, GmailAppPassword)
            };
            await client.SendMailAsync(msg);
        }
        catch (Exception)
        {
        }
    }
}
