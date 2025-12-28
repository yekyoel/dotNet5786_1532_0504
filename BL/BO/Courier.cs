namespace BO;

public class Courier
{
    public int Id { get; init; } // Courier unique identifier. Cannot be changed after creation.
    public string FullName { get; set; } // Courier full name. Can be updated.

    public string PhoneNumber { get; set; } // Courier contact phone number. Can be updated.

    public string Email { get; set; } // Courier email address. Can be updated.

    public bool IsActive { get; set; } // Indicates if the courier is currently active. Can be updated.

    public double? MaxDist { get; set; } // Maximum delivery distance the courier can cover. Can be updated.

    public ShippingMethod? ShippingMethod { get; set; } // Preferred shipping method of the courier. Can be updated.

    public DateTime? EmploymentStartDate { get; init; }  // Date when the courier started employment. Cannot be changed after creation.

    public int TotalDelSuppliedOnTime { get; init; } // Total number of deliveries supplied on time by the courier. Cannot be changed after creation.

    public int TotalDelSuppliedLate { get; init; } // Total number of deliveries supplied late by the courier. Cannot be changed after creation.

    public BO.OrderInProgress OrderInProg { get; set; } // Current order being handled by the courier. Can be updated.

}
