using Dal;
using DalApi;
using DO;
using System;

namespace DalTest;

internal class Program
{
    // Simple console test UI for the DAL (Data Access Layer)
    // Provides menus to create/read/update/delete Courier, Order and Delivery entities.

    public enum CHOICE // enumeration for user choices (maps to menu numeric options)
    {
        Exit,      // 0
        Create,    // 1
        Read,      // 2
        ReadAll,   // 3
        Update,    // 4
        Delete,    // 5
        DeleteAll  // 6
    }
    public enum ENTITY // enumeration for different main menu entities/options
    {
        Exit,       // 0
        Courier,    // 1
        Order,      // 2
        Delivery,   // 3
        Initialize, // 4 - seed sample data
        All,        // 5 - delete all data
        Config,     // 6 - view/set config clock
        Reset       // 7 - reset config
    }

    // DAL interfaces (may be null if implementations fail). These are used to call into DAL logic.
    //private static ICourier? s_dalCourier = new CourierImplementation(); //stage 1
    //private static IOrder? s_dalOrder = new OrderImplementation(); //stage 1
    //private static IDelivery? s_dalDelivery = new DeliveryImplementation(); //stage 1
    //private static IConfig? s_dalConfig = new ConfigImplementation(); //stage 1
    //static readonly IDal s_dal = new DalList(); // new DalList implementation instance (stage 2)
    //static readonly IDal s_dal = new DalXml(); // stage 3
    static readonly IDal s_dal = Factory.Get; //stage 4


    public static ENTITY COURIER { get; private set; } // unused property but left for compatibility. In case needed later.

    static void Main(string[] args)
    {
        try
        {
            menu(); // start interactive menu loop
        }
        catch (Exception ex)
        {
            // show unexpected exceptions to console
            Console.WriteLine(ex.ToString());
        }
        //}

        // Courier submenu: create, read, read-all, update, delete, delete-all
        static void CMenu()
        {
            Console.WriteLine("Courier Menu Selected");
            Console.WriteLine("Please select an action:\n" +
                    "1: Add Courier\n" +
                    "2: Read Courier\n" +
                    "3: Read All Couriers\n" +
                    "4: Update Courier\n" +
                    "5: Delete Courier\n" +
                    "0: Return to Main Menu");

            // Defensive input parsing: accept only integers in valid range
            if (!int.TryParse(Console.ReadLine(), out var choiceInt) || choiceInt < 0 || choiceInt > 6)
            {
                Console.WriteLine("Invalid selection.");
                return;
            }

            // cast integer to CHOICE enum
            CHOICE c = (CHOICE)choiceInt;

            try
            {
                switch (c)
                {
                    case CHOICE.Exit:
                        return; // Return to main menu

                    case CHOICE.Create:
                        // Gather courier fields from user (simple validation)
                        Console.Write("Id: ");
                        int.TryParse(Console.ReadLine(), out var newId);

                        Console.Write("Full Name: ");
                        var fullName = Console.ReadLine() ?? ""; // avoid null

                        Console.Write("Phone Number: ");
                        var phone = Console.ReadLine() ?? ""; // same as above

                        Console.Write("Email: ");
                        var email = Console.ReadLine() ?? "";

                        Console.Write("Is Active? (Y/N): ");
                        var isActive = (Console.ReadLine() ?? "").Trim().ToLower() == "Y";

                        Console.Write("Max Distance (optional): ");
                        double.TryParse(Console.ReadLine(), out var maxDist); // store only if valid

                        Console.Write("Preferred Shipping Method (Car/Motorcycle/Bike/Onfoot): ");
                        var prefMethodStr = Console.ReadLine() ?? "";

                        Console.Write("Day Started (dd/MM/yyyy): ");
                        DateTime.TryParse(Console.ReadLine(), out var dayStarted); // store date only if valid

                        // Create Courier record and save via DAL
                        var courier = new Courier
                        {
                            Id = newId,
                            FullName = fullName,
                            PhoneNum = phone,
                            Email = email,
                            IsActive = isActive,
                            MaxDist = double.IsNaN(maxDist) || maxDist == 0 ? null : maxDist, // if zero or invalid, store null
                            PreferredShippingMethod = prefMethodStr.ToLower() switch
                            {
                                "car" => ShippingMethod.Car,
                                "motorcycle" => ShippingMethod.Motorcycle,
                                "bike" => ShippingMethod.Bike,
                                "onfoot" => ShippingMethod.OnFoot,
                                _ => ShippingMethod.Car // unrecognized input stores default value
                            },

                            DayStarted = dayStarted == DateTime.MinValue ? (DateTime?)null : dayStarted.Date // store only date part, null if invalid

                        };
                        s_dal!.Courier.Create(courier);
                        Console.WriteLine("Courier created.");
                        break;

                    case CHOICE.Read:
                        // Read one courier by id
                        Console.Write("Enter Courier Id to read: ");
                        if (int.TryParse(Console.ReadLine(), out var readId))
                        {
                            var r = s_dal!.Courier.Read(readId);
                            Console.WriteLine(r is null ? "Courier not found." : r.ToString()!);
                        }
                        else Console.WriteLine("Invalid id.");
                        break;

                    case CHOICE.ReadAll:
                        // Read all couriers and print them
                        var all = s_dal!.Courier.ReadAll();
                        if (all is null || all.Count() == 0) Console.WriteLine("No couriers.");
                        else foreach (var it in all) Console.WriteLine(it);
                        break;

                    case CHOICE.Update:
                        // Update selected courier: keeps existing values when user presses Enter
                        Console.Write("Enter Courier Id to update: ");
                        if (int.TryParse(Console.ReadLine(), out var upId))
                        {
                            var exist = s_dal!.Courier.Read(upId);
                            if (exist is null) { Console.WriteLine("Courier not found."); break; }

                            Console.Write($"New Full Name (Enter to keep '{exist.FullName}'): ");
                            var nName = Console.ReadLine();
                            Console.Write($"New Phone Number (Enter to keep '{exist.PhoneNum}'): ");
                            var nPhone = Console.ReadLine();
                            Console.Write($"New Email (Enter to keep '{exist.Email}'): ");
                            var nEmail = Console.ReadLine();
                            Console.Write($"New Password (Enter to keep current): ");
                            var nPass = Console.ReadLine();
                            Console.Write($"Is Active? (y/N) (Enter to keep '{exist.IsActive}'): ");
                            var activeStr = Console.ReadLine();
                            Console.Write($"New Max Distance (Enter to keep '{exist.MaxDist ?? 0}'): ");
                            var nMaxStr = Console.ReadLine();
                            double.TryParse(nMaxStr, out var nMax);

                            var updated = exist with
                            {
                                // preserve existing values if input empty
                                FullName = string.IsNullOrWhiteSpace(nName) ? exist.FullName : nName,
                                PhoneNum = string.IsNullOrWhiteSpace(nPhone) ? exist.PhoneNum : nPhone,
                                Email = string.IsNullOrWhiteSpace(nEmail) ? exist.Email : nEmail,
                                IsActive = string.IsNullOrWhiteSpace(activeStr) ? exist.IsActive : (activeStr.Trim().ToLower() == "y"),
                                MaxDist = string.IsNullOrWhiteSpace(nMaxStr) ? exist.MaxDist : nMax
                            };
                            s_dal!.Courier.Update(updated);
                            Console.WriteLine("Courier updated.");
                        }
                        else Console.WriteLine("Invalid id.");
                        break;

                    case CHOICE.Delete:
                        // Delete one courier by id
                        Console.Write("Enter Courier Id to delete: ");
                        if (int.TryParse(Console.ReadLine(), out var delId))
                        {
                            s_dal!.Courier.Delete(delId);
                            Console.WriteLine("Courier deleted (if it existed).");
                        }
                        else Console.WriteLine("Invalid id.");
                        break;

                    case CHOICE.DeleteAll:
                        // Delete all couriers
                        s_dal!.Courier.DeleteAll();
                        Console.WriteLine("All couriers deleted.");
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                // Surface DAL errors to user
                Console.WriteLine($"Operation failed: {ex.Message}");
            }
        }

        // Order submenu: same pattern as courier (create/read/readall/update/delete)
        static void OMenu()
        {
            Console.WriteLine("Order Menu Selected");
            Console.WriteLine("Please select an action:\n" +
                    "1: Add Order\n" +
                    "2: Read Order\n" +
                    "3: Read All Orders\n" +
                    "4: Update Order\n" +
                    "5: Delete Order\n" +
                    "0: Return to Main Menu");

            // Defensive input parsing
            if (!int.TryParse(Console.ReadLine(), out var choiceInt) || choiceInt < 0 || choiceInt > 6)
            {
                Console.WriteLine("Invalid selection.");
                return;
            }
            // Cast to CHOICE enum
            CHOICE o = (CHOICE)choiceInt;

            try
            {
                switch (o)
                {
                    case CHOICE.Exit:
                        return;

                    case CHOICE.Create:
                        // Gather order fields (minimal validation)
                        Console.Write("Id: ");
                        int.TryParse(Console.ReadLine(), out var newId);

                        Console.Write("Latitude: ");
                        double.TryParse(Console.ReadLine(), out var lat);

                        Console.Write("Longitude: ");
                        double.TryParse(Console.ReadLine(), out var lon);

                        Console.Write("Weight: ");
                        double.TryParse(Console.ReadLine(), out var weight);

                        Console.Write("Full Address: ");
                        var fullAdd = Console.ReadLine() ?? "";

                        Console.Write("Customer Full Name: ");
                        var custName = Console.ReadLine() ?? "";

                        Console.Write("Customer Phone Number: ");
                        var custNum = Console.ReadLine() ?? "";

                        Console.Write("Description (optional): ");
                        var desc = Console.ReadLine();

                        Console.Write("Order Type (Food): ");
                        var orderTypeStr = Console.ReadLine() ?? "";

                        // Create Order record and save via DAL
                        var order = new Order
                        {
                            Id = newId,
                            Latitude = lat,
                            Longitude = lon,
                            Weight = weight,
                            FullAdd = fullAdd,
                            CustFullName = custName,
                            CusNum = custNum,
                            StartTimeForOrdering = DateTime.Now,
                            Description = string.IsNullOrWhiteSpace(desc) ? null : desc,

                            // map string input to OrderType enum, default to Pizza if unrecognized
                            Food = orderTypeStr.ToLower() switch
                            {
                                "pizza" => OrderType.Pizza,
                                "hamburger" => OrderType.Hamburger,
                                "fries" => OrderType.Fries,
                                "icecream" => OrderType.IceCream,
                                _ => OrderType.Pizza // unrecognized input stores default value
                            }
                        };
                        s_dal!.Order.Create(order); // save order
                        Console.WriteLine("Order created.");
                        break;

                    case CHOICE.Read:
                        Console.Write("Enter Order Id to read: ");
                        if (int.TryParse(Console.ReadLine(), out var readId))
                        {
                            var r = s_dal!.Order.Read(readId); // read order by id
                            Console.WriteLine(r is null ? "Order not found." : r.ToString()!); // print order or not found
                        }
                        else Console.WriteLine("Invalid id.");
                        break;
                    // Read all orders
                    case CHOICE.ReadAll:
                        var all = s_dal!.Order.ReadAll();
                        if (all is null || all.Count() == 0) Console.WriteLine("No orders.");
                        else foreach (var it in all) Console.WriteLine(it);
                        break;

                    case CHOICE.Update:
                        Console.Write("Enter Order Id to update: ");
                        if (int.TryParse(Console.ReadLine(), out var upId))
                        {
                            var exist = s_dal!.Order.Read(upId);
                            if (exist is null) { Console.WriteLine("Order not found."); break; }

                            Console.Write($"New Full Address (Enter to keep '{exist.FullAdd}'): ");
                            var nFullAdd = Console.ReadLine();
                            Console.Write($"New Description (Enter to keep '{exist.Description}'): ");
                            var nDesc = Console.ReadLine();

                            var updated = exist with
                            {
                                FullAdd = string.IsNullOrWhiteSpace(nFullAdd) ? exist.FullAdd : nFullAdd,
                                Description = string.IsNullOrWhiteSpace(nDesc) ? exist.Description : nDesc
                            };
                            s_dal!.Order?.Update(updated);
                            Console.WriteLine("Order updated.");
                        }
                        else Console.WriteLine("Invalid id.");
                        break;

                    case CHOICE.Delete:
                        Console.Write("Enter Order Id to delete: ");
                        if (int.TryParse(Console.ReadLine(), out var delId))
                        {
                            s_dal!.Order?.Delete(delId);
                            Console.WriteLine("Order deleted (if it existed).");
                        }
                        else Console.WriteLine("Invalid id.");
                        break;

                    case CHOICE.DeleteAll:
                        s_dal!.Order?.DeleteAll();
                        Console.WriteLine("All orders deleted.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Operation failed: {ex.Message}");
            }
        }

        // Delivery submenu: create/read/readall/update/delete
        static void DMenu()
        {
            // Delivery submenu: create/read/readall/update/delete
            Console.WriteLine("Delivery Menu Selected");
            Console.WriteLine("Please select an action:\n" +
                    "1: Add Delivery\n" +
                    "2: Read Delivery\n" +
                    "3: Read All Deliveries\n" +
                    "4: Update Delivery\n" +
                    "5: Delete Delivery\n" +
                    "0: Return to Main Menu");

            if (!int.TryParse(Console.ReadLine(), out var choiceInt) || choiceInt < 0 || choiceInt > 6)
            {
                Console.WriteLine("Invalid selection.");
                return;
            }

            CHOICE d = (CHOICE)choiceInt;

            try
            {
                switch (d)
                {
                    case CHOICE.Exit:
                        return;

                    case CHOICE.Create:
                        Console.Write("Id: ");
                        int.TryParse(Console.ReadLine(), out var newId);

                        Console.Write("Order Id: ");
                        int.TryParse(Console.ReadLine(), out var orderId);

                        Console.Write("Courier Id: ");
                        int.TryParse(Console.ReadLine(), out var courierId);

                        Console.Write("Distance (optional): ");
                        double.TryParse(Console.ReadLine(), out var distance);

                        Console.Write("Delivery Start Time (dd/MM/yyyy HH:mm) or Enter for now: ");
                        var startTimeStr = Console.ReadLine() ?? "";

                        Console.Write(" Delivery End Time (dd/MM/yyyy HH:mm) or Enter for now: ");
                        var endTimeStr = Console.ReadLine() ?? "";

                        Console.WriteLine("Current Delivery Status: ");
                        var statusStr = Console.ReadLine() ?? "";

                        var delivery = new Delivery
                        {
                            Id = newId,
                            OrderId = orderId,
                            CourierId = courierId,
                            Distance = double.IsNaN(distance) || distance == 0 ? null : distance,
                            DeliveryStartTime = string.IsNullOrEmpty(startTimeStr) ? DateTime.Now : DateTime.Parse(startTimeStr),
                            DeliveryEndTime = string.IsNullOrWhiteSpace(endTimeStr) ? (DateTime?)null : DateTime.Parse(endTimeStr),

                            End = statusStr.ToLower() switch
                            {
                                "pending" => CompletionType.Pending,
                                "enroute" => CompletionType.EnRoute,
                                "delivered" => CompletionType.Delivered,
                                "cancelled" => CompletionType.Cancelled,
                                "failed" => CompletionType.Failed,
                                _ => CompletionType.Pending // default if unrecognized
                            },
                        };
                        s_dal!.Delivery.Create(delivery);
                        Console.WriteLine("Delivery created.");
                        break;

                    case CHOICE.Read:
                        Console.Write("Enter Delivery Id to read: ");
                        if (int.TryParse(Console.ReadLine(), out var readId))
                        {
                            var r = s_dal!.Delivery.Read(readId);
                            Console.WriteLine(r is null ? "Delivery not found." : r.ToString()!);
                        }
                        else Console.WriteLine("Invalid id.");
                        break;

                    case CHOICE.ReadAll:
                        var all = s_dal!.Delivery.ReadAll();
                        if (all is null || all.Count() == 0) Console.WriteLine("No deliveries.");
                        else foreach (var it in all) Console.WriteLine(it);
                        break;

                    case CHOICE.Update:
                        Console.Write("Enter Delivery Id to update: ");
                        if (int.TryParse(Console.ReadLine(), out var upId))
                        {
                            var exist = s_dal!.Delivery.Read(upId);
                            if (exist is null) { Console.WriteLine("Delivery not found."); break; }

                            Console.Write($"New Distance (Enter to keep '{exist.Distance ?? 0}'): ");
                            var nDistStr = Console.ReadLine();
                            double.TryParse(nDistStr, out var nDist);

                            Console.Write("Mark delivery complete? (Y/N): ");
                            var complete = (Console.ReadLine() ?? "").Trim().ToLower() == "Y";
                            DateTime? endTime = exist.DeliveryEndTime;
                            CompletionType? endType = exist.End;
                            if (complete)
                            {
                                // mark end time and end status when user confirms completion
                                endTime = DateTime.Now;
                                endType = CompletionType.Delivered; // matches enum in context
                            }

                            var updated = exist with
                            {
                                Distance = string.IsNullOrWhiteSpace(nDistStr) ? exist.Distance : nDist,
                                DeliveryEndTime = endTime,
                                End = endType
                            };
                            s_dal!.Delivery.Update(updated);
                            Console.WriteLine("Delivery updated.");
                        }
                        else Console.WriteLine("Invalid id.");
                        break;

                    case CHOICE.Delete:
                        Console.Write("Enter Delivery Id to delete: ");
                        if (int.TryParse(Console.ReadLine(), out var delId))
                        {
                            s_dal!.Delivery.Delete(delId);
                            Console.WriteLine("Delivery deleted (if it existed).");
                        }
                        else Console.WriteLine("Invalid id.");
                        break;

                    case CHOICE.DeleteAll:
                        s_dal!.Delivery.DeleteAll();
                        Console.WriteLine("All deliveries deleted.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Operation failed: {ex.Message}");
            }
        }

        // Main menu loop: choose entity/submenu or perform global actions
        static void menu()
        { // infinite loop until user exits
            while (true)
            {
                Console.WriteLine("\n===== MAIN MENU =====");
                Console.WriteLine("0: Exit");
                Console.WriteLine("1: Courier");
                Console.WriteLine("2: Order");
                Console.WriteLine("3: Delivery");
                Console.WriteLine("4: Initialize");
                Console.WriteLine("5: Delete All");
                Console.WriteLine("6: Config (clock)");
                Console.WriteLine("7: Reset Config\n");
                Console.Write("Choose option: ");
                string? input = Console.ReadLine();
                Console.WriteLine("\n");

                // Parse user input to ENTITY enum safely
                if (!Enum.TryParse<ENTITY>(input, out var entity) || !Enum.IsDefined(typeof(ENTITY), entity))
                {
                    Console.WriteLine("Invalid choice, try again.");
                    continue;
                }

                switch (entity)
                {
                    case ENTITY.Exit:
                        Console.WriteLine("Exiting program...");
                        return;

                    case ENTITY.Courier:
                        CMenu(); // open courier submenu
                        break;

                    case ENTITY.Order:
                        OMenu(); // open order submenu
                        break;

                    case ENTITY.Delivery:
                        DMenu(); // open delivery submenu
                        break;

                    case ENTITY.Initialize:
                        //Initialization.Do(s_dalConfig, s_dalCourier, s_dalOrder, s_dalDelivery);
                        //Initialization.Do(s_dal);
                        Initialization.Do(); //stage 4
                        break;

                    case ENTITY.All:
                        // Delete all entities across DALs
                        Console.WriteLine("Deleting all entities...");
                        s_dal!.Courier.DeleteAll();
                        s_dal!.Order.DeleteAll();
                        s_dal!.Delivery.DeleteAll();
                        Console.WriteLine("All data deleted.");
                        break;

                    case ENTITY.Config:
                        // View and optionally set the DAL clock
                        Console.WriteLine($"Current Clock: {s_dal!.Config.Clock:dd/MM/yyyy HH:mm:ss}");
                        Console.Write("Set new clock (or Enter to keep): ");
                        var s = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(s) && DateTime.TryParse(s, out var newClock))
                        {
                            s_dal!.Config.Clock = newClock;
                            Console.WriteLine("Clock updated.");
                        }
                        break;

                    case ENTITY.Reset:
                        // Reset configuration to defaults
                        s_dal!.Config.Reset();
                        Console.WriteLine("Config reset.");
                        break;
                }
            }
        }
    }
}
