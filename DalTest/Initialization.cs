namespace DalTest;

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
           // Courier cr = createCouriers(i+1);
           // s_courier!.Create(cr);
        }
        
    }

    /*private static Courier createCouriers(int x) 
    {
        string[] firstNames = { "Noam", "Dana", "Avi", "Tamar", "Eli", "Shira", "Ronen", "Yael", "David", "Hila" };
        string[] lastNames = { "Levi", "Cohen", "Mizrahi", "Peretz", "Biton", "Azoulay", "Sharon", "Rosen", "Katz", "Avraham" };

        // Choose random names
        string name = $"{firstNames[s_rand.Next(firstNames.Length)]} {lastNames[s_rand.Next(lastNames.Length)]}";
        string email = name.Replace(" ", ".").ToLower() + "@gmail.com";

    }*/
            
    public static void CreateDelivery() { }

    public static void CreateOrder() { }

}

