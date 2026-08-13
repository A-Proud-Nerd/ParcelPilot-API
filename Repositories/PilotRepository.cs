using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ParcelPilot.Api.Data;
using ParcelPilot.Api.DTOs;

namespace ParcelPilot.Api.Repositories;

public class PilotRepository(AppDb db) : IPilotRepository
{
    public async Task<List<PilotDto>> GetAllAsync() =>
        (await db.Pilots
            .Where(p => p.VerificationStatus == "verified")
            .ToListAsync())
        .Select(p => p.ToDto())
        .ToList();

    public async Task<PilotDto?> GetByIdAsync(Guid id)
    {
        var pilot = await db.Pilots.FindAsync(id);
        return pilot?.ToDto();
    }

    public async Task<PilotDto?> UpdateProfileAsync(Guid pilotId, UpdatePilotProfileRequest req)
    {
        var pilot = await db.Pilots.FirstOrDefaultAsync(p => p.Id == pilotId);
        if (pilot is null)
            return null;

        var user = await db.Users.FirstOrDefaultAsync(u => u.Role == "pilot" && u.ProfileId == pilotId);
        if (user is null)
            return null;

        var isVerified = pilot.VerificationStatus == "verified";

        if (isVerified)
        {
            if (!string.IsNullOrWhiteSpace(req.City))
            {
                pilot.ZonesJson = JsonSerializer.Serialize(new[] { req.City.Trim() });
            }

            if (!string.IsNullOrWhiteSpace(req.Email))
            {
                if (await db.Users.AnyAsync(u => u.Email == req.Email.Trim() && u.Id != user.Id))
                    throw new InvalidOperationException("Email already registered.");

                user.Email = req.Email.Trim();
            }

            if (!string.IsNullOrWhiteSpace(req.Phone))
                pilot.Phone = req.Phone.Trim();
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(req.Name))
                pilot.Name = req.Name.Trim();

            if (!string.IsNullOrWhiteSpace(req.Email))
            {
                if (await db.Users.AnyAsync(u => u.Email == req.Email.Trim() && u.Id != user.Id))
                    throw new InvalidOperationException("Email already registered.");

                user.Email = req.Email.Trim();
            }

            if (!string.IsNullOrWhiteSpace(req.Phone))
                pilot.Phone = req.Phone.Trim();

            if (!string.IsNullOrWhiteSpace(req.City))
                pilot.ZonesJson = JsonSerializer.Serialize(new[] { req.City.Trim() });

            if (!string.IsNullOrWhiteSpace(req.VehicleType))
                pilot.VehicleType = req.VehicleType.Trim();

            var hasRequiredProfileData =
                !string.IsNullOrWhiteSpace(pilot.Name) &&
                !string.IsNullOrWhiteSpace(user.Email) &&
                !string.IsNullOrWhiteSpace(pilot.Phone) &&
                !string.IsNullOrWhiteSpace(pilot.VehicleType) &&
                !string.IsNullOrWhiteSpace(pilot.ZonesJson);

            pilot.VerificationStatus = hasRequiredProfileData ? "verified" : "pending";
        }

        await db.SaveChangesAsync();
        return pilot.ToDto();
    }
}
