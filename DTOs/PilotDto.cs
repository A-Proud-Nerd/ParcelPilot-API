namespace ParcelPilot.Api.DTOs;

public record PilotDto(
    int Id,
    string Name,
    decimal Rating,
    int CompletedDeliveries,
    int ReliabilityScore,
    string VehicleType,
    string VehiclePlate,
    string[] Zones,
    bool IsPreferred,
    bool IsOnline,
    string VerificationStatus,
    decimal PerKmRate,
    decimal BaseFee,
    string Phone
);
