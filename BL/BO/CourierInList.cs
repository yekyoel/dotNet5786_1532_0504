using DO;

namespace BO;

public class CourierInList
{
    public int CourierId { get; init; } // Identifier for the courier
    public string FullName { get; init; } // Full name of the courier
    public bool IsActive { get; init; } // Indicates if the courier is currently active
    public OrderType TypeOrder { get; init; } // Most associated order type for the courier
    public DateTime? EmploymentStartDate { get; init; } // Employment start date of the courier
    public int TotalDelSuppliedOnTime { get; init; } // Total number of deliveries supplied on time by the courier
    public int TotalLateDelSupplied { get; init; } // Total number of deliveries supplied late by the courier
    public int OrderId { get; init; } // Current order ID being handled by the courier
}
