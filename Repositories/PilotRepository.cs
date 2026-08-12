using Microsoft.EntityFrameworkCore;
using ParcelPilot.Api.Data;
using ParcelPilot.Api.DTOs;

namespace ParcelPilot.Api.Repositories;

public class PilotRepository(AppDb db) : IPilotRepository
{
    public async Task<List<PilotDto>> GetAllAsync() =>
        (await db.Pilots.ToListAsync()).Select(p => p.ToDto()).ToList();

    public async Task<PilotDto?> GetByIdAsync(Guid id)
    {
        var pilot = await db.Pilots.FindAsync(id);
        return pilot?.ToDto();
    }
}
