using Dal;
using DalApi;
using DO;
using System;



namespace DalTest;

internal class Program
{
    public enum CHOICE // enumeration for user choices
    {
        Exit,
        Create,
        Read,
        ReadAll,
        Update,
        Delete,
        DeleteAll
    }
    public enum ENTITY // enumeration for different entities
    {
        Exit,
        Courier,
        Order,
        Delivery,
        Initialize,
        All,
        Config,
        Reset
    }
   



    private static ICourier? s_dalCourier = new CourierImplementation(); //stage 1
    private static IOrder? s_dalOrder = new OrderImplementation(); //stage 1
    private static IDelivery? s_dalDelivery = new DeliveryImplementation(); //stage 1
    private static IConfig? s_dalConfig = new ConfigImplementation(); //stage 1

    public static ENTITY COURIER { get; private set; }

    static void Main(string[] args)
    {
        try
        {
            menu();


        }
        catch (Exception ex) 
        { 
            Console.WriteLine(ex.ToString());            
        }
    }
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
        CHOICE c = (CHOICE)int.Parse(Console.ReadLine()!); // Read user input and convert to CHOICE enum
        switch (c)
        {
            case CHOICE.Exit:
                return; //  Exit the submenu
                
            case CHOICE.Create:
                
                break;

                case CHOICE.Read:
                    break;

                case CHOICE.ReadAll:

                    break;

                case CHOICE.Update:

                    break;

                case CHOICE.Delete:

                break;

                case CHOICE.DeleteAll:

                    break;


            default: break;
        }




    }

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
        CHOICE o = (CHOICE)int.Parse(Console.ReadLine()!); // Read user input and convert to CHOICE enum
        switch (o)
        {
            case CHOICE.Exit:
                return; //  Exit the submenu
            case CHOICE.Create:
                break;
            case CHOICE.Read:
                break;
            case CHOICE.ReadAll:
                break;
            case CHOICE.Update:
                break;
            case CHOICE.Delete:
                break;
            case CHOICE.DeleteAll:
                break;

        }

    }
      static private void DMenu()
    {
        Console.WriteLine("Delivery Menu Selected");
            Console.WriteLine("Please select an action:\n" +
                "1: Add Delivery\n" +
                "2: Read Delivery\n" +
                "3: Read All Deliveries\n" +
                "4: Update Delivery\n" +
                "5: Delete Delivery\n" +
                "0: Return to Main Menu");
            CHOICE d = (CHOICE)int.Parse(Console.ReadLine()!); // Read user input and convert to CHOICE enum
            switch (d)
            {
                case CHOICE.Exit:
                    return; //  Exit the submenu
                case CHOICE.Create:
                    break;
                case CHOICE.Read:
                    break;
                case CHOICE.ReadAll:
                    break;
                case CHOICE.Update:
                    break;
                case CHOICE.Delete:
                    break;
                case CHOICE.DeleteAll:
                    break;
            }

        }
    
      static private void menu() {

        Console.WriteLine("Please select Menu:\n" +
                "1: Courier\n" +
                "2: Order\n" +
                "3: Delivery\n" +
                "0: Exit Program");
        ENTITY choice = (ENTITY)int.Parse(Console.ReadLine()!); // Read user input and convert to ENTITY enum

        switch (choice)
        {
            case ENTITY.Courier:
                CMenu(); // Example call to subMenu with ADD choice
                break;
            case ENTITY.Order:
                OMenu();
                break;
            case ENTITY.Delivery:
                DMenu();
                break;
            case ENTITY.Exit:
                Console.WriteLine("Exiting program.");
                break;



            default: break;
        }


      }
}
