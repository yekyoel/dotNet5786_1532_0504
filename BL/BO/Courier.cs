namespace BO;

public class Courier
{
    public int Id { get; init; } // Courier unique identifier
    public string FullName { get; set; } // Courier full name

    public string PhoneNumber { get; set; } // Courier contact phone number

    public string Email { get; set; } // Courier email address

    public bool IsActive { get; set; } // Indicates if the courier is currently active

    public double? MaxDist { get; set; } // Maximum delivery distance the courier can cover

    public ShippingMethod? ShippingMethod { get; set; } // Preferred shipping method of the courier

    public DateTime? EmploymentStartDate { get; init; }  // 

    public int TotalDelSuppliedOnTime { get; init; } // Total number of deliveries supplied on time by the courier

    public int TotalDelSuppliedLate { get; init; } // Total number of deliveries supplied late by the courier

    BO.OrderInProgress OrderInProg { get; set; } // Current order being handled by the courier

}
