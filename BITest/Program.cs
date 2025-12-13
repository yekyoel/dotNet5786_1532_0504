using BlApi; // TODO: adjust namespace
using BO;
using System;


namespace BITest;

class Program
{
    static readonly IBl s_bl = Factory.Get(); // TODO: adjust Factory name

    public static void Main()
    {
        bool exit = false;

        while (!exit)
        {
            Console.WriteLine("\n===== MAIN MENU =====");
            Console.WriteLine("1 - Admin");
            Console.WriteLine("2 - Student");
            // TODO: add other logical entities here
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
                    StudentMenu();
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
                        s_bl.Admin.ForwardClock(TimeUnit.HOUR); // TODO: ask user if needed
                        Console.WriteLine("Clock forwarded");
                        break;

                    case 4:
                        Console.WriteLine(s_bl.Admin.GetClock());
                        break;

                    case 5:
                        var config = s_bl.Admin.GetConfig();
                        Console.WriteLine("Configuration:");
                        Console.WriteLine($"MAX_RANGE = {config.MaxRange}");
                        break;

                    case 6:
                        Console.Write("Enter new MAX_RANGE: ");
                        if (int.TryParse(Console.ReadLine(), out int newMaxRange))
                        {
                            s_bl.Admin.SetConfig(new Config { MaxRange = newMaxRange });
                            Console.WriteLine("Config updated");
                        }
                        break;

                    case 0:
                        exit = true;
                        break;
                }
            }
            catch (Exception ex) // TODO: catch specific BO exceptions if required
            {
                PrintException(ex);
            }
        }
    }

    private static void StudentMenu()
    {
        bool exit = false;

        while (!exit)
        {
            Console.WriteLine("\n--- STUDENT MENU ---");
            Console.WriteLine("1 - Read student");
            Console.WriteLine("2 - Read all students");
            Console.WriteLine("0 - Back");

            Console.Write("Choose option: ");
            if (!int.TryParse(Console.ReadLine(), out int choice))
                continue;

            try
            {
                switch (choice)
                {
                    case 1:
                        Console.Write("Enter student id: ");
                        if (int.TryParse(Console.ReadLine(), out int id))
                        {
                            Student? stud = s_bl.Student.Read(id);
                            Console.WriteLine(stud);
                        }
                        break;

                    case 2:
                        foreach (var item in s_bl.Student.ReadAll())
                            Console.WriteLine(item);
                        break;

                    case 0:
                        exit = true;
                        break;
                }
            }
            catch (BlDoesNotExistException ex)
            {
                PrintException(ex);
            }
        }
    }
}
