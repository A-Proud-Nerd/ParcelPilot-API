namespace ParcelPilot.Api.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = ""; // "business" | "pilot"
    public Guid ProfileId { get; set; }     // BusinessId or PilotId
}
