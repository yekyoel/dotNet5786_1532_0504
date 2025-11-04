// Module Order.cs
namespace DO;
/// <summary>
/// Entity representing an order in the system.
/// </summary>
/// <param name="Id">Unique identifier for the order</param>
/// <param name="Latitude">Geographical latitude of the delivery location</param>
/// <param name="Longitude">Geographical longitude of the delivery location</param>
/// <param name="Weight">Weight of the order in kilograms</param>
/// <param name="FullAdd">Full address for the delivery location</param>
/// <param name="CustFullName">Full name of the customer placing the order</param>
/// <param name="CusNum">Contact number of the customer</param>
/// <param name="StartTimeForOrdering">Timestamp when the order was placed</param>
/// <param name="Description">Optional description of the order</param>
/// <param name="Food">Optional type of food ordered</param>
public record Order
(
    int Id, 
    double Latitude, 
    double Longitude, 
    double Weight, 
    string FullAdd, 
    string CustFullName, 
    string CusNum, 
    DateTime? StartTimeForOrdering = null,  
    string? Description = null,  
    OrderType? Food = null  
)
/// <summary>
/// Default constructor initializing an Order with default values.
/// <summary>
{
    public Order() : this(0,0.0,0.0,0.0,"","","") { }
}
