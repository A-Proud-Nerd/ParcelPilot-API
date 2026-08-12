using Microsoft.EntityFrameworkCore;
using ParcelPilot.Api.Data;
using ParcelPilot.Api.Models;

namespace ParcelPilot.Api.Repositories;

public class BusinessRepository(AppDb db) : IBusinessRepository
{
    public Task<List<Business>> GetAllAsync() => db.Businesses.ToListAsync();
}
