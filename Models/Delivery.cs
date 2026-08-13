namespace ParcelPilot.Api.Models;

public class Delivery
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BusinessId { get; set; }
    public string Reference { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal WeightKg { get; set; }
    public string Priority { get; set; } = "standard";
    public bool RequiresSignature { get; set; }
    public string Currency { get; set; } = "ZAR";
    public string Status { get; set; } = "awaiting_approval";
    public Guid? AssignedPilotId { get; set; }
    public Guid? RequestedPilotId { get; set; }
    public DateTime? RequestedAt { get; set; }
    public bool IsPublic { get; set; } = false;
    public decimal AgreedSubtotal { get; set; }
    public decimal ServiceFee { get; set; }
    public decimal TotalCharge { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ScheduledFor { get; set; }
    public List<DeliveryStop> Stops { get; set; } = [];
    public List<Quote> Quotes { get; set; } = [];
}
