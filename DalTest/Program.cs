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
    private static ICourier? s_dalCourier = new CourierImplementation(); //stage 1
    private static IOrder? s_dalOrder = new OrderImplementation(); //stage 1
    private static IDelivery? s_dalDelivery = new DeliveryImplementation(); //stage 1
    private static IConfig? s_dalConfig = new ConfigImplementation(); //stage 1

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
    }

    // Courier submenu: create, read, read-all, update, delete, delete-all
    static private void CMenu()
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
                    var fullName = Console.ReadLine() ?? "";

                    Console.Write("Phone Number: ");
                    var phone = Console.ReadLine() ?? "";

                    Console.Write("Email: ");
                    var email = Console.ReadLine() ?? "";

                    Console.Write("Password: ");
                    var password = Console.ReadLine() ?? "";

                    Console.Write("Is Active? (y/N): ");
                    var isActive = (Console.ReadLine() ?? "").Trim().ToLower() == "y";

                    Console.Write("Max Distance (optional): ");
                    double.TryParse(Console.ReadLine(), out var maxDist);

                    // Create Courier record and save via DAL
                    var courier = new Courier
                    {
                        Id = newId,
                        FullName = fullName,
                        PhoneNum = phone,
                        Email = email,
                        IsActive = isActive,
                        MaxDist = double.IsNaN(maxDist) || maxDist == 0 ? null : maxDist
                    };
                    s_dalCourier?.Create(courier);
                    Console.WriteLine("Courier created.");
                    break;

                case CHOICE.Read:
                    // Read one courier by id
                    Console.Write("Enter Courier Id to read: ");
                    if (int.TryParse(Console.ReadLine(), out var readId))
                    {
                        var r = s_dalCourier?.Read(readId);
                        Console.WriteLine(r is null ? "Courier not found." : r.ToString()!);
                    }
                    else Console.WriteLine("Invalid id.");
                    break;

                case CHOICE.ReadAll:
                    // Read all couriers and print them
                    var all = s_dalCourier?.ReadAll();
                    if (all is null || all.Count == 0) Console.WriteLine("No couriers.");
                    else foreach (var it in all) Console.WriteLine(it);
                    break;

                case CHOICE.Update:
                    // Update selected courier: keeps existing values when user presses Enter
                    Console.Write("Enter Courier Id to update: ");
                    if (int.TryParse(Console.ReadLine(), out var upId))
                    {
                        var exist = s_dalCourier?.Read(upId);
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
                        s_dalCourier?.Update(updated);
                        Console.WriteLine("Courier updated.");
                    }
                    else Console.WriteLine("Invalid id.");
                    break;

                case CHOICE.Delete:
                    // Delete one courier by id
                    Console.Write("Enter Courier Id to delete: ");
                    if (int.TryParse(Console.ReadLine(), out var delId))
                    {
                        s_dalCourier?.Delete(delId);
                        Console.WriteLine("Courier deleted (if it existed).");
                    }
                    else Console.WriteLine("Invalid id.");
                    break;

                case CHOICE.DeleteAll:
                    // Delete all couriers
                    s_dalCourier?.DeleteAll();
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
    static private void OMenu()
    {
        Console.WriteLine("Order Menu Selected");
        Console.WriteLine("Please select an action:\n" +
                "1: Add Order\n" +
                "2: Read Order\n" +
                "3: Read All Orders\n" +
                "4: Update Order\n" +
                "5: Delete Order\n" +
                "0: Return to Main Menu");

        if (!int.TryParse(Console.ReadLine(), out var choiceInt) || choiceInt < 0 || choiceInt > 6)
        {
            Console.WriteLine("Invalid selection.");
            return;
        }

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
                        Description = string.IsNullOrWhiteSpace(desc) ? null : desc
                    };
                    s_dalOrder?.Create(order);
                    Console.WriteLine("Order created.");
                    break;

                case CHOICE.Read:
                    Console.Write("Enter Order Id to read: ");
                    if (int.TryParse(Console.ReadLine(), out var readId))
                    {
                        var r = s_dalOrder?.Read(readId);
                        Console.WriteLine(r is null ? "Order not found." : r.ToString()!);
                    }
                    else Console.WriteLine("Invalid id.");
                    break;

                case CHOICE.ReadAll:
                    var all = s_dalOrder?.ReadAll();
                    if (all is null || all.Count == 0) Console.WriteLine("No orders.");
                    else foreach (var it in all) Console.WriteLine(it);
                    break;

                case CHOICE.Update:
                    Console.Write("Enter Order Id to update: ");
                    if (int.TryParse(Console.ReadLine(), out var upId))
                    {
                        var exist = s_dalOrder?.Read(upId);
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
                        s_dalOrder?.Update(updated);
                        Console.WriteLine("Order updated.");
                    }
                    else Console.WriteLine("Invalid id.");
                    break;

                case CHOICE.Delete:
                    Console.Write("Enter Order Id to delete: ");
                    if (int.TryParse(Console.ReadLine(), out var delId))
                    {
                        s_dalOrder?.Delete(delId);
                        Console.WriteLine("Order deleted (if it existed).");
                    }
                    else Console.WriteLine("Invalid id.");
                    break;

                case CHOICE.DeleteAll:
                    s_dalOrder?.DeleteAll();
                    Console.WriteLine("All orders deleted.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Operation failed: {ex.Message}");
        }
    }
      static private void DMenu()
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

                    var delivery = new Delivery
                    {
                        Id = newId,
                        OrderId = orderId,
                        CourierId = courierId,
                        Distance = double.IsNaN(distance) || distance == 0 ? null : distance,
                        DeliveryStartTime = DateTime.Now
                    };
                    s_dalDelivery?.Create(delivery);
                    Console.WriteLine("Delivery created.");
                    break;

                case CHOICE.Read:
                    Console.Write("Enter Delivery Id to read: ");
                    if (int.TryParse(Console.ReadLine(), out var readId))
                    {
                        var r = s_dalDelivery?.Read(readId);
                        Console.WriteLine(r is null ? "Delivery not found." : r.ToString()!);
                    }
                    else Console.WriteLine("Invalid id.");
                    break;

                case CHOICE.ReadAll:
                    var all = s_dalDelivery?.ReadAll();
                    if (all is null || all.Count == 0) Console.WriteLine("No deliveries.");
                    else foreach (var it in all) Console.WriteLine(it);
                    break;

                case CHOICE.Update:
                    Console.Write("Enter Delivery Id to update: ");
                    if (int.TryParse(Console.ReadLine(), out var upId))
                    {
                        var exist = s_dalDelivery?.Read(upId);
                        if (exist is null) { Console.WriteLine("Delivery not found."); break; }

                        Console.Write($"New Distance (Enter to keep '{exist.Distance ?? 0}'): ");
                        var nDistStr = Console.ReadLine();
                        double.TryParse(nDistStr, out var nDist);

                        Console.Write("Mark delivery complete? (y/N): ");
                        var complete = (Console.ReadLine() ?? "").Trim().ToLower() == "y";
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
                        s_dalDelivery?.Update(updated);
                        Console.WriteLine("Delivery updated.");
                    }
                    else Console.WriteLine("Invalid id.");
                    break;

                case CHOICE.Delete:
                    Console.Write("Enter Delivery Id to delete: ");
                    if (int.TryParse(Console.ReadLine(), out var delId))
                    {
                        s_dalDelivery?.Delete(delId);
                        Console.WriteLine("Delivery deleted (if it existed).");
                    }
                    else Console.WriteLine("Invalid id.");
                    break;

                case CHOICE.DeleteAll:
                    s_dalDelivery?.DeleteAll();
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
    static private void menu()
    {
        while (true)
        {
            Console.WriteLine("\n===== MAIN MENU =====");
            Console.WriteLine("0: Exit");
            Console.WriteLine("1: Courier");
            Console.WriteLine("2: Order");
            Console.WriteLine("3: Delivery");
            Console.WriteLine("4: Initialize (seed data)");
            Console.WriteLine("5: Delete All");
            Console.WriteLine("6: Config (clock)");
            Console.WriteLine("7: Reset Config");

            Console.Write("Choose option: ");
            string? input = Console.ReadLine();

            // ניסיון להמיר ל־enum ENTITY
            // Parse user input to ENTITY enum safely
            if (!Enum.TryParse<ENTITY>(input, out var entity) || !Enum.IsDefined(typeof(ENTITY), entity))
            {
                Console.WriteLine("⚠️ Invalid choice, try again.");
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
                    // Seed sample data (safe helper)
                    Console.WriteLine("Initializing sample data...");
                    SeedSampleData();
                    break;

                case ENTITY.All:
                    // Delete all entities across DALs
                    Console.WriteLine("Deleting all entities...");
                    s_dalCourier!.DeleteAll();
                    s_dalOrder!.DeleteAll();
                    s_dalDelivery!.DeleteAll();
                    Console.WriteLine("✅ All data deleted.");
                    break;

                case ENTITY.Config:
                    // View and optionally set the DAL clock
                    Console.WriteLine($"Current Clock: {s_dalConfig!.Clock:dd/MM/yyyy HH:mm:ss}");
                    Console.Write("Set new clock (or Enter to keep): ");
                    var s = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(s) && DateTime.TryParse(s, out var newClock))
                    {
                        s_dalConfig.Clock = newClock;
                        Console.WriteLine("✅ Clock updated.");
                    }
                    break;

                case ENTITY.Reset:
                    // Reset configuration to defaults
                    s_dalConfig!.Reset();
                    Console.WriteLine("✅ Config reset.");
                    break;
            }
        }
    }
    private static void SeedSampleData()
    {
        // Small helper to add sample records to DAL for testing
        s_dalCourier?.Create(new Courier { Id = 1, FullName = "Sample Courier", PhoneNum = "000-000-0000", Email = "sample@courier.com", IsActive = true });
        s_dalOrder?.Create(new Order { Id = 1, Description = "Sample Order" });
        s_dalDelivery?.Create(new Delivery { Id = 1, CourierId = 1, OrderId = 1 });
        Console.WriteLine("Sample data seeded.");
    }

}
