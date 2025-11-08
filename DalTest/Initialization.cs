namespace DalTest;

using System;
using DalApi;
using DO;

public static  class Initialization
{
    private static IConfig? s_dalConfig;
    private static ICourier? s_courier;
    private static IDelivery? s_delivery;
    private static IOrder? s_order;

    private static readonly Random s_rand = new();

    public static void CreateConfig() 
    {
       

    }

    public static void CreateCourier() 
    {
        for (int i = 0; i < 20; i++)
        {
            Courier cr = createCouriers(i+1);
            s_courier?.Create(cr);
        }
        
    }

    public static DateTime randDate()
    {
        DateTime start = new DateTime(s_dalConfig.Clock.Year - 2, 1, 1);
        int range = (s_dalConfig.Clock - start ).Days;
        return start.AddDays(s_rand.Next(range));
    }

    private static Courier createCouriers(int x)
    {
        string[] firstNames = { "Noam", "Dana", "Avi", "Tamar", "Eli", "Shira", "Ronen", "Yael", "David", "Hila" };
        string[] lastNames = { "Levi", "Cohen", "Mizrahi", "Peretz", "Biton", "Azoulay", "Sharon", "Rosen", "Katz", "Avraham" };

        // Random name/email
        string name = $"{firstNames[s_rand.Next(firstNames.Length)]} {lastNames[s_rand.Next(lastNames.Length)]}";
        string email = name.Replace(" ", ".").ToLower() + "@gmail.com";

        // 9-digit random ID
        int id = s_rand.Next(200_000_000, 400_000_000);


        // Phone like 05X-XXXXXXX
        string phone = $"05{s_rand.Next(0, 10)}-{s_rand.Next(1_000_000, 10_000_000)}";

        // Most couriers to be active (80% chance)
        bool isActive = s_rand.NextDouble() < 0.8;

        // Random preferred shipping method (enum values assumed 0..3)
        ShippingMethod preferred = (ShippingMethod)s_rand.Next(0, 4);


        DateTime dayStarted = randDate(); 

        // MaxDist: choose a reasonable random value (not too far).
        // Use config max if set, otherwise default cap 30 km. Minimum 1 km.
        double configCap = s_dalConfig?.MaxDist ?? 30.0;
        double cap = Math.Min(Math.Max(configCap, 1.0), 30.0); // ensure between 1 and 30
        double maxDist = Math.Round(1.0 + s_rand.NextDouble() * (cap - 1.0), 1); // 1.0 .. cap, 1 decimal place

        return new Courier
        {
            Id = id,
            FullName = name,
            PhoneNum = phone,
            Email = email,
            IsActive = isActive,
            MaxDist = maxDist,
            PreferredShippingMethod = preferred,
            DayStarted = dayStarted
        };
    }



    public static void CreateDelivery() 
    {
        for (int i = 0; i < 50; i++)
        {
            Delivery cr = createDeliveries(i + 1);
            s_delivery?.Create(cr);
        }
    }

    public static DateTime RandomDeliveryOpenTime()
    {
        // Current time (using your system or DAL clock)
        DateTime now = s_dalConfig.Clock;

        // Choose a "start" boundary — say, up to 30 days ago
        int maxDaysBack = 30;

        // Pick a random number of days/hours/minutes ago
        int daysBack = s_rand.Next(maxDaysBack);        // 0–29 days
        int hoursBack = s_rand.Next(24);                // 0–23 hours
        int minutesBack = s_rand.Next(60);              // 0–59 minutes

        // Subtract that random offset from now
        DateTime randomTime = now.AddDays(-daysBack)
                                 .AddHours(-hoursBack)
                                 .AddMinutes(-minutesBack);

        return randomTime;
    }

    private static Delivery createDeliveries(int x)
    {
        // Random preferred shipping method (enum values assumed 0..3)
        ShippingMethod preferred = (ShippingMethod)s_rand.Next(0, 4);

        DateTime delStartTime? = RandomDeliveryOpenTime();
        DateTime delEndTime? = RandomDeliveryOpenTime();
        while (delEndTime <= delStartTime)
        {
            delEndTime = RandomDeliveryOpenTime();
        }

        CompletionType? end = null;
        if (x <= 20)
        {
            end = 0; // Pending
        }
        else if (x > 20 && x <= 30)
        {
            end = 1; // enroute
        }
        else
        {
            end = s_rand.Next(2, 5); // Delivered, Cancelled, Failed
        }

        return new Delivery
        {
            CourierId = s_rand.Next(200_000_000, 400_000_000),
            ShippingMethod = preferred,
            DeliveryStartTime = delStartTime,
            Distance = Math.Round(1.0 + s_rand.NextDouble() * 49.0, 1), // 1.0 .. 50.0 km  FIX!!!
            End = end,
            DeliveryEndTime = delEndTime
        };
    }



    public static void CreateOrder()
    {

    }

}

