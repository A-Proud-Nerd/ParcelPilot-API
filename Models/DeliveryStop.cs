namespace ParcelPilot.Api.Models;

public class DeliveryStop
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DeliveryId { get; set; }
    public int Sequence { get; set; }
    public string Type { get; set; } = "";
    public string ContactName { get; set; } = "";
    public string ContactPhone { get; set; } = "";
    public string AddressLine1 { get; set; } = "";
    public string City { get; set; } = "";
    public string? Instructions { get; set; }
    public string Status { get; set; } = "pending";
}
