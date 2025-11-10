namespace DalTest;

using DalApi;
using DO;
using System;
using System.Net;



public static  class Initialization
{
    private static IConfig? s_dalConfig;
    private static ICourier? s_courier;
    private static IDelivery? s_delivery;
    private static IOrder? s_order;

    private static readonly Random s_rand = new();



    private static double DegreeToRad(double deg) => deg * (Math.PI / 180.0);
    private static double HaversineDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        //Radius of Earth Glob
        double R = 6371;
        double dLat = DegreeToRad(lat2 - lat1);
        double dLon = DegreeToRad(lon2 - lon1);
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(DegreeToRad(lat1)) * Math.Cos(DegreeToRad(lat2)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    public static void CreateConfig() 
    {
        s_dalConfig.AdminId = 123456789; // Fixed admin ID for testing
        s_dalConfig.CompanyName = "FastFood4You";

        //"Ha-Va'ad Ha-Leumi, Jerusalem"
        s_dalConfig.Latitude = 31.76417;
        s_dalConfig.Longitude = 35.22534;

        s_dalConfig.MaxDelTime = TimeSpan.FromMinutes(40);// 30 minutes
        s_dalConfig.RiskRange = TimeSpan.FromMinutes(10); // 10 minutes
        s_dalConfig.DownTime = TimeSpan.FromMinutes(20); // 20 minutes
                                                   
        s_dalConfig.MaxDist = 20.0;
        s_dalConfig.AvgCarMPH = 70.0;
        s_dalConfig.AvgMotorcycleMPH = 50.0;
        s_dalConfig.AvgBicycleMPH = 15.0;
        s_dalConfig.AvgWalkMPH = 5.0;

    }



    public static void CreateCourier() 
    {
        for (int i = 0; i < 20; i++)
        {
            Courier cr = createCouriers();
            if (s_courier?.Read(cr.Id) == null)
                s_courier?.Create(cr);
        }
        
    }

    public static DateTime randDate()
    {
        DateTime start = new DateTime(s_dalConfig.Clock.Year - 2, 1, 1);
        int range = (s_dalConfig.Clock - start ).Days;
        return start.AddDays(s_rand.Next(range));
    }

    private static Courier createCouriers()
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







    public static string GenerateRandomOrder(OrderType type)
    {
        string description = type switch
        {
            OrderType.Pizza => RandomFrom(new[]
            {
                "Large pepperoni pizza with extra cheese",
                "Margherita pizza with fresh basil",
                "BBQ chicken pizza with onion and olives",
                "Veggie pizza loaded with mushrooms and peppers"
            }),

            OrderType.Hamburger => RandomFrom(new[]
            {
                "Double cheeseburger with fries",
                "Classic beef burger with lettuce and tomato",
                "Spicy chicken burger with mayo",
                "Vegan burger with avocado and sprouts"
            }),

            OrderType.Fries => RandomFrom(new[]
            {
                "Crispy golden fries with ketchup",
                "Curly fries with garlic dip",
                "Sweet potato fries with aioli",
                "Loaded fries with cheese and bacon bits"
            }),

            OrderType.IceCream => RandomFrom(new[]
            {
                "Chocolate ice cream with caramel drizzle",
                "Vanilla scoop with rainbow sprinkles",
                "Mint chip cone with chocolate topping",
                "Strawberry sundae with whipped cream"
            }),

            _ => "Unknown order"
        };

        return (description);
    }

    private static string RandomFrom(string[] options)
    {
        return options[s_rand.Next(options.Length)];
    }

    public static DateTime RandomDeliveryOpenTime()
    {
        // Current time (using your system or DAL clock)
        DateTime now = s_dalConfig.Clock;


        // Pick a random number of hours/minutes ago
        int hoursBack = s_rand.Next(24);                // 0–23 hours
        int minutesBack = s_rand.Next(60);              // 0–59 minutes

        // Subtract that random offset from now
        DateTime randomTime = now.AddHours(-hoursBack)
                                   .AddMinutes(-minutesBack);

        return randomTime;
    }

    public static void CreateOrder()
    {
        for (int i = 0; i < 50; i++)
        {
            Order cr = createOrders();
            if (s_order?.Read(cr.Id) == null)
                s_order?.Create(cr);
        }
    }

    public static Order createOrders()
    {
        DateTime orderTimeStart = RandomDeliveryOpenTime();
        OrderType type = (OrderType)s_rand.Next(0,4);
        string description = GenerateRandomOrder(type);

        double weight = Math.Round(0.5 + s_rand.NextDouble() * 4.5, 2); // 0.5 to 5.0 kg

        string[] firstNames = { "Noam", "Dana", "Avi", "Tamar", "Eli", "Shira", "Ronen", "Yael", "David", "Hila" };
        string[] lastNames = { "Levi", "Cohen", "Mizrahi", "Peretz", "Biton", "Azoulay", "Sharon", "Rosen", "Katz", "Avraham" };

        // Random name/email
        string name = $"{firstNames[s_rand.Next(firstNames.Length)]} {lastNames[s_rand.Next(lastNames.Length)]}";
        string phone = $"05{s_rand.Next(0, 10)}-{s_rand.Next(1_000_000, 10_000_000)}";

        string[] addresses = {
            "16 Jaffa St, Jerusalem, Israel",
            "5 Beit HaKerem St, Jerusalem, Israel",
            "3 Herzl Blvd, Jerusalem, Israel",
            "18 Emek Refaim St, Jerusalem, Israel",
            "2 Ramot Rd, Jerusalem, Israel",
            "12 Hebron Rd, Jerusalem, Israel",
            "8 Pierre Koenig St, Talpiot, Jerusalem, Israel",
            "14 Mount Herzl St, Jerusalem, Israel",
            "10 Har Homa Blvd, Jerusalem, Israel",
            "22 Ben Gurion Blvd, Mevaseret Zion, Israel"
        };

        double[] latitudes = {
            31.7821, 31.7729, 31.7742, 31.7617, 31.8204,
            31.7519, 31.7464, 31.7613, 31.7191, 31.8060
        };

        double[] longitudes = {
            35.2193, 35.1877, 35.2031, 35.2248, 35.2009,
            35.2327, 35.2195, 35.1828, 35.2340, 35.1550
        };

        int index = s_rand.Next(addresses.Length);

        string address = addresses[index];
        double lat = latitudes[index];
        double lon = longitudes[index];

        return new Order
        {
            Latitude = lat,
            Longitude = lon,
            Weight = weight,
            FullAdd = address,
            CustFullName = name,
            CusNum = phone,
            StartTimeForOrdering = orderTimeStart,
            Description = description,
            Food = type
        };
    }


    public static void CreateDelivery()
    {
        // assume s_dalConfig, s_order, s_courier, s_delivery and other values are correct and non-null

        var orders = s_order.ReadAll();
        if (orders == null || orders.Count == 0) return;

        var couriers = s_courier.ReadAll();
        var existingDeliveries = s_delivery.ReadAll() ?? new List<Delivery>();

        // pick one random order from the list
        var order = orders[s_rand.Next(orders.Count)];

        // compute distance from store to customer
        double storeLat = s_dalConfig.Latitude ?? 0.0;
        double storeLon = s_dalConfig.Longitude ?? 0.0;
        double distanceKm = Math.Round(HaversineDistanceKm(storeLat, storeLon, order.Latitude, order.Longitude), 2);

        // eligible couriers: active and within max distance
        var eligible = couriers.Where(c => c.IsActive && (c.MaxDist == null || c.MaxDist >= distanceKm)).ToList();
        if (eligible.Count == 0)
            return; // no courier can serve this order

        // pick a random eligible courier
        var courier = eligible[s_rand.Next(eligible.Count)];

        // compute courier available-from based on existing deliveries (avoid overlapping assignments)
        DateTime courierAvailableFrom = s_dalConfig.Clock;
        var courierDeliveries = existingDeliveries.Where(d => d.CourierId == courier.Id).ToList();
        if (courierDeliveries.Count > 0)
        {
            DateTime latest = courierDeliveries
                .Select(d =>
                {
                    if (d.DeliveryEndTime.HasValue) return d.DeliveryEndTime.Value;
                    if (d.DeliveryStartTime.HasValue)
                    {
                        // estimate end using simple speed estimate based on recorded distance and shipping method
                        double dKm = d.Distance ?? distanceKm;
                        var method = d.ShippingMethod ?? courier.PreferredShippingMethod ?? ShippingMethod.Car;
                        double speedKmh = method switch
                        {
                            ShippingMethod.Car => s_dalConfig.AvgCarMPH,
                            ShippingMethod.Motorcycle => s_dalConfig.AvgMotorcycleMPH,
                            ShippingMethod.Bike => s_dalConfig.AvgBicycleMPH,
                            ShippingMethod.OnFoot => s_dalConfig.AvgWalkMPH,
                            _ => s_dalConfig.AvgCarMPH
                        };
                        if (speedKmh <= 0) speedKmh = 30.0;
                        var duration = TimeSpan.FromHours((dKm) / speedKmh);
                        return d.DeliveryStartTime.Value + duration;
                    }
                    return s_dalConfig.Clock;
                })
                .Max();
            courierAvailableFrom = latest;
        }

        // earliest start must be after order start and courier availability and after downtime
        DateTime earliest = (order.StartTimeForOrdering ?? s_dalConfig.Clock) > courierAvailableFrom ? (order.StartTimeForOrdering ?? s_dalConfig.Clock) : courierAvailableFrom;
        earliest = earliest.Add(s_dalConfig.DownTime);

        // small random scheduling delay
        DateTime start = earliest.AddMinutes(s_rand.Next(0, 16));

        // estimate duration using courier preferred method
        var chosenMethod = courier.PreferredShippingMethod ?? ShippingMethod.Car;
        double speed = chosenMethod switch
        {
            ShippingMethod.Car => s_dalConfig.AvgCarMPH,
            ShippingMethod.Motorcycle => s_dalConfig.AvgMotorcycleMPH,
            ShippingMethod.Bike => s_dalConfig.AvgBicycleMPH,
            ShippingMethod.OnFoot => s_dalConfig.AvgWalkMPH,
            _ => s_dalConfig.AvgCarMPH
        };
        if (speed <= 0) speed = 30.0;
        TimeSpan estimatedDuration = TimeSpan.FromHours(distanceKm / speed);

        // pick completion type with reasonable probabilities
        double r = s_rand.NextDouble();
        CompletionType completion = r < 0.15 ? CompletionType.Pending
                                : r < 0.35 ? CompletionType.EnRoute
                                : r < 0.85 ? CompletionType.Delivered
                                : r < 0.925 ? CompletionType.Cancelled
                                : CompletionType.Failed;

        DateTime? endTime = null;
        if (completion == CompletionType.Delivered || completion == CompletionType.Cancelled || completion == CompletionType.Failed)
        {
            // finish time = start + estimatedDuration + small random extra minutes
            endTime = start + estimatedDuration + TimeSpan.FromMinutes(s_rand.Next(0, 21));
        }

        var delivery = new Delivery
        {
            OrderId = order.Id,
            CourierId = courier.Id,
            ShippingMethod = chosenMethod,
            DeliveryStartTime = start,
            Distance = distanceKm,
            End = completion,
            DeliveryEndTime = endTime
        };

        // persist delivery and remove order so it won't be reused
        s_delivery.Create(delivery);
        s_order.Delete(order.Id);
    }
    


    public static void Do(IConfig? dalConfig, ICourier? dalCourier, IOrder? dalOrder, IDelivery? dalDelivery)
    {
        s_dalConfig = dalConfig ?? throw new NullReferenceException("DAL can not be null!");
        s_courier = dalCourier ?? throw new NullReferenceException("DAL Courier can not be null!");
        s_order = dalOrder ?? throw new NullReferenceException("DAL Order can not be null!");
        s_delivery = dalDelivery ?? throw new NullReferenceException("DAL Delivery can not be null!");

        Console.WriteLine("Reset Configuration values and List values...");
        s_dalConfig.Reset();
        s_courier.DeleteAll();
        s_order.DeleteAll();
        s_delivery.DeleteAll();

        Console.WriteLine("Initializing Couriers list ...");
        CreateCourier();
        Console.WriteLine("Initializing Orders list ...");
        CreateOrder();
        Console.WriteLine("Initializing Deliveries list ...");
        CreateDelivery();
        Console.WriteLine("Initializing Config ...");
        CreateConfig();


    }
}

