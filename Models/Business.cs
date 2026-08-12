namespace ParcelPilot.Api.Models;

public class Business
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Industry { get; set; } = "";
    public string Address { get; set; } = "";
    public string VerificationStatus { get; set; } = "pending";
}
