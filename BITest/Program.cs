using BlApi; 
using BO;
using System;


namespace BITest;

class Program
{
    static readonly IBl s_bl = Factory.Get(); 

    public static void Main()
    {
        bool exit = false;

        while (!exit)
        {
            Console.WriteLine("\n===== MAIN MENU =====");
            Console.WriteLine("1 - Admin");
            Console.WriteLine("2 - Courier");
            Console.WriteLine("3 - Order");
            Console.WriteLine("0 - Exit");

            Console.Write("Choose option: ");
            if (!int.TryParse(Console.ReadLine(), out int choice))
                continue;

            switch (choice)
            {
                case 1:
                    AdminMenu();
                    break;
                case 2:
                    CourierMenu();
                    break;
                case 3:
                    OrderMenu();
                    break;
                case 0:
                    exit = true;
                    break;
            }
        }

    }

    private static void AdminMenu()
    {
        bool exit = false;

        while (!exit)
        {
            Console.WriteLine("\n--- ADMIN MENU ---");
            Console.WriteLine("1 - Reset DB");
            Console.WriteLine("2 - Initialize DB");
            Console.WriteLine("3 - Forward Clock");
            Console.WriteLine("4 - Get Clock");
            Console.WriteLine("5 - Get Config");
            Console.WriteLine("6 - Set Config");
            Console.WriteLine("0 - Back");

            Console.Write("Choose option: ");
            if (!int.TryParse(Console.ReadLine(), out int choice))
                continue;

            try
            {
                switch (choice)
                {
                    case 1:
                        s_bl.Admin.ResetDB();
                        Console.WriteLine("DB reset");
                        break;

                    case 2:
                        s_bl.Admin.InitializeDB();
                        Console.WriteLine("DB initialized");
                        break;

                    case 3:
                        Console.WriteLine("Forward Clock by: 1-Minute, 2-Hour, 3-Day, 4-Month, 5-Year");
                        if (int.TryParse(Console.ReadLine(), out int timeUnit))
                        {
                            var time = timeUnit switch
                            {
                                1 => Time.Minute,
                                2 => Time.Hour,
                                3 => Time.Day,
                                4 => Time.Month,
                                5 => Time.Year,
                                _ => Time.Hour
                            };
                            s_bl.Admin.ForwardClock(time);
                            Console.WriteLine("Clock forwarded");
                        }
                        break;

                    case 4:
                        Console.WriteLine($"Current Clock: {s_bl.Admin.GetClock()}");
                        break;

                    case 5:
                        var config = s_bl.Admin.GetConfig();
                        Console.WriteLine("Configuration:");
                        Console.WriteLine($"Admin ID: {config.AdminId}");
                        Console.WriteLine($"Company Name: {config.CompanyName}");
                        Console.WriteLine($"Max Delivery Time: {config.MaxDelTime}");
                        Console.WriteLine($"Risk Range: {config.RiskRange}");
                        break;

                    case 6:
                        Console.WriteLine("Update config settings (not fully implemented in this menu)");
                        break;

                    case 0:
                        exit = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private static void CourierMenu()
    {
        bool exit = false;

        while (!exit)
        {
            Console.WriteLine("\n--- COURIER MENU ---");
            Console.WriteLine("1 - View Couriers List");
            Console.WriteLine("2 - Get Courier Details");
            Console.WriteLine("0 - Back");

            Console.Write("Choose option: ");
            if (!int.TryParse(Console.ReadLine(), out int choice))
                continue;

            try
            {
                switch (choice)
                {
                    case 1:
                        Console.WriteLine("Viewing all couriers...");
                        var couriers = s_bl.Courier.GetListOfCouriers(0, null, null);
                        foreach (var courier in couriers)
                        {
                            Console.WriteLine($"ID: {courier.CourierId}, Name: {courier.FullName}, Active: {courier.IsActive}");
                        }
                        break;

                    case 2:
                        Console.Write("Enter courier ID: ");
                        if (int.TryParse(Console.ReadLine(), out int courierId))
                        {
                            var courier = s_bl.Courier.GetCourierDetails(0, courierId);
                            Console.WriteLine($"Courier: {courier.FullName}, Email: {courier.Email}, Phone: {courier.PhoneNumber}");
                        }
                        break;

                    case 0:
                        exit = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private static void OrderMenu()
    {
        bool exit = false;

        while (!exit)
        {
            Console.WriteLine("\n--- ORDER MENU ---");
            Console.WriteLine("1 - View All Orders");
            Console.WriteLine("2 - Get Order Details");
            Console.WriteLine("0 - Back");

            Console.Write("Choose option: ");
            if (!int.TryParse(Console.ReadLine(), out int choice))
                continue;

            try
            {
                switch (choice)
                {
                    case 1:
                        Console.WriteLine("Viewing all orders...");
                        var orders = s_bl.Order.GetListOfOrders(0, null, null, null);
                        foreach (var order in orders)
                        {
                            Console.WriteLine($"Order ID: {order.OrderId}, Type: {order.OrderType}, Status: {order.OrderStatus}");
                        }
                        break;

                    case 2:
                        Console.Write("Enter order ID: ");
                        if (int.TryParse(Console.ReadLine(), out int orderId))
                        {
                            var order = s_bl.Order.GetOrderDetails(0, orderId);
                            Console.WriteLine($"Order: {order.Id}, Customer: {order.CustomerName}, Address: {order.OrderAddress}");
                        }
                        break;

                    case 0:
                        exit = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
