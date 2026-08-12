namespace ParcelPilot.Api.Models;

public class Quote
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DeliveryId { get; set; }
    public Guid PilotId { get; set; }
    public string PilotName { get; set; } = "";
    public decimal PilotRating { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal DistanceAmount { get; set; }
    public decimal Subtotal { get; set; }
    public string? Note { get; set; }
    public string Status { get; set; } = "proposed";
    public DateTime SubmittedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
