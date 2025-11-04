
namespace DO;

public record Order
(
    int Id, //**
    // enum OrderType
    string FullAdd,
    double Latitude,
    double Longitude,
    string CustFullName,
    string CusNum,
    double Weight,
    DateTime StartTimeForOrdering,
    string? Description = null
);
