using System.Text.Json;
using ParcelPilot.Api.DTOs;

namespace ParcelPilot.Api.Models;

public class Pilot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public decimal Rating { get; set; }
    public int CompletedDeliveries { get; set; }
    public int ReliabilityScore { get; set; }
    public string VehicleType { get; set; } = "";
    public string VehiclePlate { get; set; } = "";
    public string ZonesJson { get; set; } = "[]";
    public bool IsPreferred { get; set; }
    public bool IsOnline { get; set; }
    public string VerificationStatus { get; set; } = "pending";
    public decimal PerKmRate { get; set; }
    public decimal BaseFee { get; set; }
    public string Phone { get; set; } = "";

    public PilotDto ToDto() => new(
        Id, Name, Rating, CompletedDeliveries, ReliabilityScore,
        VehicleType, VehiclePlate,
        JsonSerializer.Deserialize<string[]>(ZonesJson) ?? [],
        IsPreferred, IsOnline, VerificationStatus, PerKmRate, BaseFee, Phone
    );
}
