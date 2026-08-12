using ParcelPilot.Api.DTOs;

namespace ParcelPilot.Api.Repositories;

public interface IAuthRepository
{
    Task<(AuthResponse? response, string? error)> RegisterBusinessAsync(RegisterBusinessRequest req);
    Task<(AuthResponse? response, string? error)> RegisterPilotAsync(RegisterPilotRequest req);
    Task<(AuthResponse? response, string? error)> LoginAsync(LoginRequest req);
}
