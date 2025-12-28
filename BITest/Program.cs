using BlApi; 
using BO;
using System;


namespace BITest;

class Program
{
    static readonly IBl s_bl = Factory.Get();  // Singleton BL instance
    static int s_userId; // Store userId at class level

    public static void Main()
    {
        Console.WriteLine("Enter Your Id:");
        if (!int.TryParse(Console.ReadLine(), out s_userId))
        {
            Console.WriteLine("Invalid ID. Exiting...");
            return;
        }
        
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
                    AdminMenu(); // Call Admin Menu
                    break;
                case 2:
                    CourierMenu(); // Call Courier Menu
                    break;
                case 3:
                    OrderMenu(); // Call Order Menu
                    break;
                case 0:
                    exit = true;
                    break;
            }
        }

    }

    /// <summary>
    /// Displays the administrative menu and processes user input for performing administrative operations such as
    /// resetting or initializing the database, forwarding the system clock, and managing configuration settings.
    /// </summary>
    /// <remarks>This method provides a console-based interface for administrators to perform system-level
    /// tasks. It is intended for use in interactive scenarios and blocks execution until the user chooses to exit the
    /// menu. The method handles invalid input and displays error messages for exceptions encountered during
    /// administrative operations.</remarks>
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
                        Console.WriteLine("Forward Clock by: 1-Second, 2-Minute, 3-Hour, 4-Day, 5-Year");
                        if (int.TryParse(Console.ReadLine(), out int timeUnit))
                        {
                            var time = timeUnit switch
                            {
                                1 => Time.Minute,
                                2 => Time.Hour,
                                3 => Time.Day,
                                //4 => Time.Month,
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

    /// <summary>
    /// Displays the courier management menu and handles user interactions for viewing couriers and retrieving courier
    /// details.
    /// </summary>
    /// <remarks>This method presents a console-based menu that allows users to view a list of couriers or
    /// obtain details for a specific courier. The method continues to prompt for input until the user chooses to exit
    /// the menu. This method is intended for interactive console applications and blocks execution until the user exits
    /// the menu.</remarks>
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
                        var couriers = s_bl.Courier.GetListOfCouriers(s_userId, null, null);
                        foreach (var courier in couriers)
                        {
                            Console.WriteLine($"ID: {courier.CourierId}, Name: {courier.FullName}, Active: {courier.IsActive}");
                        }
                        break;

                    case 2:
                        Console.Write("Enter courier ID: ");
                        if (int.TryParse(Console.ReadLine(), out int courierId))
                        {
                            var courier = s_bl.Courier.GetCourierDetails(s_userId, courierId);
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

    /// <summary>
    /// Displays the order management menu and processes user input for viewing orders and order details.
    /// </summary>
    /// <remarks>This method presents a console-based menu that allows users to view all orders or retrieve
    /// details for a specific order. The menu continues to prompt for input until the user chooses to exit. Intended
    /// for interactive console applications.</remarks>
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
                        var orders = s_bl.Order.GetListOfOrders(s_userId, null, null, null);
                        foreach (var order in orders)
                        {
                            Console.WriteLine($"Order ID: {order.OrderId}, Type: {order.OrderType}, Status: {order.OrderStatus}");
                        }
                        break;

                    case 2:
                        Console.Write("Enter order ID: ");
                        if (int.TryParse(Console.ReadLine(), out int orderId))
                        {
                            var order = s_bl.Order.GetOrderDetails(s_userId, orderId);
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
