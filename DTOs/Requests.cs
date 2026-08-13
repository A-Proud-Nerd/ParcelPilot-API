namespace ParcelPilot.Api.DTOs;

public record CreateDeliveryRequest(
    string PickupAddress,
    string DropoffAddress,
    string Description,
    string? PickupContactName,
    string? PickupContactPhone,
    string? PickupCity,
    string? DropoffContactName,
    string? DropoffContactPhone,
    string? DropoffCity,
    decimal WeightKg,
    string? Priority,
    bool RequiresSignature,
    bool IsPublic = false,
    Guid? PilotId = null
);

public record SubmitQuoteRequest(
    Guid PilotId,
    decimal BaseAmount,
    decimal DistanceAmount,
    string? Note,
    int? ExpiresInMinutes
);

public record AssignPilotRequest(Guid PilotId);

public record RespondToRequestRequest(bool Accept);

public record UpdatePilotProfileRequest(
    string? Name,
    string? Email,
    string? Phone,
    string? City,
    string? VehicleType
);

public record UpdateStatusRequest(string Status);
