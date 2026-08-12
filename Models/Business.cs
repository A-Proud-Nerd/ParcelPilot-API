namespace ParcelPilot.Api.Models;

public class Business
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string Industry { get; set; } = "";
    public string Address { get; set; } = "";
    public string VerificationStatus { get; set; } = "pending";
}
