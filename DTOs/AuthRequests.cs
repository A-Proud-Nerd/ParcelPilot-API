namespace ParcelPilot.Api.DTOs;

public record RegisterBusinessRequest(
    string Email,
    string Password,
    string Name,
    string Industry,
    string Address,
    string? RegistrationNumber
);

public record RegisterPilotRequest(
    string Email,
    string Password,
    string Name,
    string Phone,
    string City,
    string VehicleType,
    string? IdNumber
);

public record LoginRequest(string Email, string Password);

public record AuthResponse(
    string Token,
    string Role,
    int ProfileId,
    string Name
);
