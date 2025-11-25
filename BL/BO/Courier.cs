namespace BO;

public class Courier
{
    public int Id { get; init; }
    public string FullName { get; set; }

    public string PhoneNumber { get; set; }

    public string Email { get; set; }

    public string Password { get; set;}

    public bool IsActive { get; set;}

    public double? MaxDist { get; set;}

    public OrderType orderType { get; set;}

    public DateTime EmploymentStartDate { get; init; } 

    public int TotalDelSuppliedOnTime { get; init; }

    public int TotalDelSuppliedLate { get; init; }

    //BO.OrderInProgress huhuh // nullable

}
