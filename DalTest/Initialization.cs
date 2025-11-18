namespace DalTest;

using DalApi;
using DO;
using System;
using System.Diagnostics.Metrics;
using System.Linq; // added for ElementAt, Count, Where, etc.
using System.Net;


/// <summary>
/// Initialization class to setup initial data in the DAL for testing.
/// <param name= "s_dal">The data access layer instance to initialize.</param>
/// <param name= "s_rand">Random number generator for creating random data.</param>
/// <param name= "MIN_ID">Minimum ID value for generated entities.</param>
/// <param name= "MAX_ID">Maximum ID value for generated entities.</param>
/// </summary>
public static  class Initialization
{
    //private static IConfig? s_dalConfig; //stage 1
    //private static ICourier? s_courier; //stage 1
    //private static IDelivery? s_delivery; //stage 1
    //private static IOrder? s_order; //stage 1
    private static IDal? s_dal; //stage 2
    private static readonly Random s_rand = new();
    private const int MIN_ID = 200000000;
    private const int MAX_ID = 400000000;

    /// <summary>
    /// Converts degrees to radians.
    /// </summary>
    /// <param name="deg"></param>
    /// <returns></returns>
    private static double DegreeToRad(double deg) => deg * (Math.PI / 180.0);

    /// <summary>
    ///  calculates the Haversine distance between two geographic coordinates in kilometers.
    /// </summary>
    /// <param name="lat1"></param>
    /// <param name="lon1"></param>
    /// <param name="lat2"></param>
    /// <param name="lon2"></param>
    /// <returns> double </returns>
    /// 
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

    /// <summary>
    /// Initializes and configures default settings for the application.
    /// <param name= "AdminId">The admin ID to set in the configuration.</param>
    /// <param name= "CompanyName">The company name to set in the configuration.</param>
    /// <param name= "Latitude">The latitude coordinate for the store location.</param>
    /// <param name="Longitude">The longitude coordinate for the store location.</param>
    /// <param name= "MaxDelTime">The maximum delivery time allowed.</param>
    /// <param name= "RiskRange">The risk range for deliveries.</param>
    /// <param name= "DownTime">The downtime between deliveries.</param>
    /// <param name= "MaxDist">The maximum distance a courier can travel.</param>
    /// <param name= "AvgCarMPH">The average speed of a car in miles per hour.</param>
    /// <param name= "AvgMotorcycleMPH">The average speed of a motorcycle in miles per hour.</param>
    /// <param name= "AvgBicycleMPH">The average speed of a bicycle in miles per hour.</param>
    /// <param name= "AvgWalkMPH">The average walking speed in miles per hour.</param>
    /// <summary>
    public static void CreateConfig() 
    {
        s_dal!.Config.AdminId = 123456789; // Fixed admin ID for testing
        s_dal!.Config.CompanyName = "FastFood4You";

        //"Ha-Va'ad Ha-Leumi, Jerusalem"
        s_dal!.Config.Latitude = 31.76417;
        s_dal!.Config.Longitude = 35.22534;

        s_dal!.Config.MaxDelTime = TimeSpan.FromMinutes(40);// 30 minutes
        s_dal!.Config.RiskRange = TimeSpan.FromMinutes(10); // 10 minutes
        s_dal!.Config.DownTime = TimeSpan.FromMinutes(20); // 20 minutes
                                                   
        s_dal!.Config.MaxDist = 20.0;
        s_dal!.Config.AvgCarMPH = 70.0;
        s_dal!.Config.AvgMotorcycleMPH = 50.0;
        s_dal!.Config.AvgBicycleMPH = 15.0;
        s_dal!.Config.AvgWalkMPH = 5.0;

    }

    /// <summary>
    /// creates and adds random couriers to the DAL for testing purposes.
    /// </summary>
    public static void CreateCourier() 
    {
        for (int i = 0; i < 20; i++)
        {
            Courier cr = createCouriers(); // create a random courier
            if (s_dal!.Courier.Read(cr.Id) == null) // check for existing courier by ID
                s_dal!.Courier.Create(cr); 
        }
        
    }

    /// <summary>
    /// Generates a random date within the last two years from the current DAL clock.
    /// </summary>
    /// <returns> DateTime </returns>
    public static DateTime randDate()
    {
        DateTime start = new DateTime(s_dal!.Config.Clock.Year - 2, 1, 1);
        int range = (s_dal!.Config.Clock - start ).Days;
        return start.AddDays(s_rand.Next(range));
    }

    /// <summary>
    /// Generates a random Courier object with various attributes.
    /// </summary>
    /// <returns></returns>
    private static Courier createCouriers()
    {
        string[] firstNames = { "Noam", "Dana", "Avi", "Tamar", "Eli", "Shira", "Ronen", "Yael", "David", "Hila" };
        string[] lastNames = { "Levi", "Cohen", "Mizrahi", "Peretz", "Biton", "Azoulay", "Sharon", "Rosen", "Katz", "Avraham" };

        // Random name/email
        string name = $"{firstNames[s_rand.Next(firstNames.Length)]} {lastNames[s_rand.Next(lastNames.Length)]}";
        string email = name.Replace(" ", ".").ToLower() + "@gmail.com";

        // 9-digit random ID
        int id = s_rand.Next(MIN_ID, MAX_ID);


        // Phone like 05X-XXXXXXX
        string phone = $"05{s_rand.Next(0, 10)}-{s_rand.Next(1_000_000, 10_000_000)}";

        // Most couriers to be active (80% chance)
        bool isActive = s_rand.NextDouble() < 0.8;

        // Random preferred shipping method (enum values assumed 0..3)
        ShippingMethod preferred = (ShippingMethod)s_rand.Next(0, 4);


        DateTime dayStarted = randDate(); 

        // MaxDist: choose a reasonable random value (not too far).
        // Use config max if set, otherwise default cap 30 km. Minimum 1 km.
        double configCap = s_dal!.Config.MaxDist ?? 30.0;
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



    /// <summary>
    /// Generates a random order description based on the specified order type.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Selects a random string from the provided array of options.
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    private static string RandomFrom(string[] options)
    {
        return options[s_rand.Next(options.Length)];
    }

    /// <summary>
    /// Generates a random delivery open time within the last 24 hours from the current DAL clock.
    /// </summary>
    /// <returns></returns>
    public static DateTime RandomDeliveryOpenTime()
    {
        // Current time (using your system or DAL clock)
        DateTime now = s_dal!.Config.Clock;


        // Pick a random number of hours/minutes ago
        int hoursBack = s_rand.Next(24);                // 0–23 hours
        int minutesBack = s_rand.Next(60);              // 0–59 minutes

        // Subtract that random offset from now
        DateTime randomTime = now.AddHours(-hoursBack)
                                   .AddMinutes(-minutesBack);

        return randomTime;
    }

    /// <summary>
    /// creates and adds random orders to the DAL for testing purposes.
    /// </summary>
    public static void CreateOrder()
    {
        for (int i = 0; i < 50; i++) 
        {
            Order cr = createOrders();
            s_dal!.Order.Create(cr);
        }
    }

    /// <summary>
    /// Generates a random Order object with various attributes.
    /// </summary>
    /// <returns></returns>
    public static Order createOrders()
    {
        DateTime orderPlacedTime = s_dal!.Config.Clock; // current time from DAL clock
        OrderType type = (OrderType)s_rand.Next(0,4);// enum values 0..3
        string description = GenerateRandomOrder(type);// description based on type

        double weight = Math.Round(0.5 + s_rand.NextDouble() * 4.5, 2); // 0.5 to 5.0 kg

        string[] firstNames = { "Noam", "Dana", "Avi", "Tamar", "Eli", "Shira", "Ronen", "Yael", "David", "Hila" };
        string[] lastNames = { "Levi", "Cohen", "Mizrahi", "Peretz", "Biton", "Azoulay", "Sharon", "Rosen", "Katz", "Avraham" };

        // Random name/email
        string name = $"{firstNames[s_rand.Next(firstNames.Length)]} {lastNames[s_rand.Next(lastNames.Length)]}";
        string phone = $"05{s_rand.Next(0, 10)}-{s_rand.Next(1_000_000, 10_000_000)}";// Phone like 05X-XXXXXXX

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
        }; // sample addresses in Jerusalem area

        double[] latitudes = {
            31.7821, 31.7729, 31.7742, 31.7617, 31.8204,
            31.7519, 31.7464, 31.7613, 31.7191, 31.8060
        };// sample latitudes corresponding to addresses

        double[] longitudes = {
            35.2193, 35.1877, 35.2031, 35.2248, 35.2009,
            35.2327, 35.2195, 35.1828, 35.2340, 35.1550
        };// sample longitudes corresponding to addresses

        int index = s_rand.Next(addresses.Length);// pick a random address

        string address = addresses[index];// get corresponding address
        double lat = latitudes[index];// get corresponding latitude
        double lon = longitudes[index];// get corresponding longitude

        return new Order
        {
            Id = 0, // ID will be assigned by DAL
            Latitude = lat,
            Longitude = lon,
            Weight = weight,
            FullAdd = address,
            CustFullName = name,
            CusNum = phone,
            StartTimeForOrdering = orderPlacedTime,
            Description = description,
            Food = type
        };
    }

    /// <summary>
    /// Calculates the Haversine distance between two geographic coordinates.
    /// </summary>
    public static void CreateDelivery()
    {
        List<DO.Order> orders = s_dal!.Order.ReadAll().ToList();
        List<DO.Courier> couriers = s_dal!.Courier.ReadAll().ToList();
        List<DO.Delivery> existingDeliveries = s_dal!.Delivery.ReadAll().ToList();

        double storeLat = s_dal!.Config.Latitude ?? 0.0;
        double storeLon = s_dal!.Config.Longitude ?? 0.0;

        do
        {
            var order = orders[s_rand.Next(orders.Count)];
            double distanceKm = Math.Round(HaversineDistanceKm(storeLat, storeLon, order.Latitude, order.Longitude), 2);


            // eligible couriers: active and within max distance
            var eligible = couriers.Where(c => c.IsActive && (c.MaxDist == null || c.MaxDist >= distanceKm)).ToList();
            if (eligible.Count == 0)
                return ; // no courier can serve this order Error

            var courier = eligible[s_rand.Next(eligible.Count)]; // pick a random eligible courier

            // compute courier available-from based on existing deliveries (avoid overlapping assignments)
            DateTime courierAvailableFrom = s_dal!.Config.Clock;
            var courierDeliveries = existingDeliveries.Where(d => d.CourierId == courier.Id).ToList();

            if (courierDeliveries.Count > 0)
            {
                // find latest end time among courier's deliveries
                DateTime latest = courierDeliveries
                    .Select(d =>
                    {
                        if (d.DeliveryEndTime.HasValue) return d.DeliveryEndTime.Value;// use actual end time if available
                        if (d.DeliveryStartTime.HasValue) // estimate end time if only start time is available
                        {
                            // estimate end using simple speed estimate based on recorded distance and shipping method
                            double dKm = d.Distance ?? distanceKm;
                            var method = d.ShippingMethod ?? courier.PreferredShippingMethod ?? ShippingMethod.Car;
                            double speedKmh = method switch
                            {
                                ShippingMethod.Car => s_dal!.Config.AvgCarMPH,
                                ShippingMethod.Motorcycle => s_dal!.Config.AvgMotorcycleMPH,
                                ShippingMethod.Bike => s_dal!.Config.AvgBicycleMPH,
                                ShippingMethod.OnFoot => s_dal!.Config.AvgWalkMPH,
                                _ => s_dal!.Config.AvgCarMPH
                            };
                            if (speedKmh <= 0) speedKmh = 30.0;
                            var duration = TimeSpan.FromHours((dKm) / speedKmh);
                            return d.DeliveryStartTime.Value + duration;
                        }
                        return s_dal!.Config.Clock;
                    })
                    .Max();
                courierAvailableFrom = latest;
            }

            // earliest start must be after order start and courier availability and after downtime
            DateTime earliest = (order.StartTimeForOrdering ?? s_dal!.Config.Clock) > courierAvailableFrom ? (order.StartTimeForOrdering ?? s_dal!.Config.Clock) : courierAvailableFrom;
            earliest = earliest.Add(s_dal!.Config.DownTime);

            // small random scheduling delay
            DateTime start = earliest.AddMinutes(s_rand.Next(0, 16));

            // estimate duration using courier preferred method
            var chosenMethod = courier.PreferredShippingMethod ?? ShippingMethod.Car;
            double speed = chosenMethod switch
            {
                ShippingMethod.Car => s_dal!.Config.AvgCarMPH,
                ShippingMethod.Motorcycle => s_dal!.Config.AvgMotorcycleMPH,
                ShippingMethod.Bike => s_dal!.Config.AvgBicycleMPH,
                ShippingMethod.OnFoot => s_dal!.Config.AvgWalkMPH,
                _ => s_dal!.Config.AvgCarMPH
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
                Id = 0, // ID will be assigned by DAL
                OrderId = order.Id,
                CourierId = courier.Id,
                ShippingMethod = chosenMethod,
                DeliveryStartTime = start,
                Distance = distanceKm,
                End = completion,
                DeliveryEndTime = endTime
            };

            // persist delivery and remove order so it won't be reused
            s_dal!.Delivery.Create(delivery);
            s_dal!.Order.Delete(order.Id);
            orders = s_dal!.Order.ReadAll().ToList();
        } while (orders.Count() != 0);
    }


    /// <summary>
    /// Initializes the DAL with default configuration and sample data.
    /// </summary>
    /// <param name="dal"></param>
    /// <exception cref="NullReferenceException"></exception>
    public static void Do(IDal dal)
    {
        //s_dalConfig = dalConfig ?? throw new NullReferenceException("DAL can not be null!");
        //s_courier = dalCourier ?? throw new NullReferenceException("DAL Courier can not be null!");
        //s_order = dalOrder ?? throw new NullReferenceException("DAL Order can not be null!");
        //s_delivery = dalDelivery ?? throw new NullReferenceException("DAL Delivery can not be null!");

        s_dal = dal ?? throw new DalCanNotBeNullException("DAL object can not be null!"); 

        Console.WriteLine("Reset Configuration values and List values...");
        //s_dalConfig.Reset();
        //s_courier.DeleteAll();
        //s_order.DeleteAll();
        //s_delivery.DeleteAll();

        s_dal.ResetDB(); // reset all data in the DAL

        Console.WriteLine("Initializing Config ...");
        CreateConfig();
        Console.WriteLine("Initializing Couriers list ...");
        CreateCourier();
        Console.WriteLine("Initializing Orders list ...");
        CreateOrder();
        Console.WriteLine("Initializing Deliveries list ...");
        CreateDelivery();
        
  
    }
}

