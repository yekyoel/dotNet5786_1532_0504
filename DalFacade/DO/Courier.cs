// Module Courier.cs
namespace DO;
/// <summary>
/// Entity that represents a courier in the delivery system.
/// </summary>
/// <param name="Id">Personal unique ID of the courier</param>
/// <param name="FullName">Full name of the courier</param>
/// <param name="PhoneNum">Contact phone number of the courier</param>
/// <param name="Email">Contact email address of the courier</param>
/// <param name="Password">Password for courier's account</param>
/// <param name="IsActive">Indicates if the courier is currently active</param>
/// <param name="MaxDist">Maximum distance the courier is willing to travel</param>
/// <param name="PreferredShippingMethod">Preferred method of shipping for the courier</param>
/// <param name="DayStarted">Date when the courier started working</param>
public record Courier
(
    int Id,
    string FullName, 
    string PhoneNum, 
    string Email, 
    //string Password, 
    bool IsActive = false,  
    double? MaxDist = null,
    ShippingMethod? PreferredShippingMethod = null, 
    DateTime? DayStarted = null 
)

/// <summary>
/// Default constructor for Courier with default values.
/// <summary>
{
    public Courier() : this(0, "", "", ""){ }
}

