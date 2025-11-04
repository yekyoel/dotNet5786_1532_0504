
namespace DO;

public record Order
(
    int Id, //**
    // enum OrderType
    double Latitude,
    double Longitude,
    double Weight,
    string FullAdd,
    string CustFullName,
    string CusNum,
    DateTime StartTimeForOrdering,
    string? Description = null
)
{
    public Order() : this(0,0,0,0,"","","",new(0,0,0)) { }
}
