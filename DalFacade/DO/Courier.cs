

namespace DO;

public record Courier
   (
    int Id,
    string FullName,
    string PhoneNum,
    string Email,
    string Password,
    bool IsActive = false,
    double? MaxDist = null,
    //enum DELTYPE { CAR , MOTORBIKE, BIKE, ONFOOT},
    DateTime? DayStarted = null   /// check
    )
{
    public Courier() : this(0, "", "", "", ""){ }
}

