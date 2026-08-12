using ParcelPilot.Api.DTOs;

namespace ParcelPilot.Api.Repositories;

public interface IPilotRepository
{
    Task<List<PilotDto>> GetAllAsync();
    Task<PilotDto?> GetByIdAsync(int id);
}
