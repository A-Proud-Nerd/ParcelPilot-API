using ParcelPilot.Api.Models;

namespace ParcelPilot.Api.Repositories;

public interface IBusinessRepository
{
    Task<List<Business>> GetAllAsync();
}
