using DO;

namespace BO;

public class CourierInList
{
    public int CourierId { get; init; }
    public string FullName { get; init; }
    public bool IsActive { get; init; }
    public OrderType TypeOrder { get; init; }
    public DateTime? EmploymentStartDate { get; init; }
    public int TotalDelSuppliedOnTime { get; init; }
    public int TotalLateDelSupplied { get; init; }
    public int OrderId { get; init; }
}
