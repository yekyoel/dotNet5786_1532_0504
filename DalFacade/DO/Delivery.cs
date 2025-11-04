
namespace DO;

public record Delivery
(
    int Id,//**
    int OrderId,//**
    int CourierId,
    //DeliveryType 
    DateTime DeliveryStartTime, 
    double? Distance = null,
    //DeliveryEndType = null,
    DateTime? DeliveryEndTime = null
)
{
    /// <summary>
    /// Default constructor for stage 3
    /// </summary>
    public Delivery() :this(0,0,0, new DateTime(0, 0, 0)) { }
}