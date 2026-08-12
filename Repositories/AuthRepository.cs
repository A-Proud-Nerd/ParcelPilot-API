using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ParcelPilot.Api.Data;
using ParcelPilot.Api.DTOs;
using ParcelPilot.Api.Models;

namespace ParcelPilot.Api.Repositories;

public class AuthRepository(AppDb db, IConfiguration config) : IAuthRepository
{
    public async Task<(AuthResponse? response, string? error)> RegisterBusinessAsync(RegisterBusinessRequest req)
    {
        if (await db.Users.AnyAsync(u => u.Email == req.Email))
            return (null, "Email already registered.");

        var business = new Business
        {
            Name = req.Name,
            Industry = req.Industry,
            Address = req.Address,
            VerificationStatus = "pending"
        };
        db.Businesses.Add(business);
        await db.SaveChangesAsync();

        var user = new User
        {
            Email = req.Email,
            PasswordHash = BCrypt.HashPassword(req.Password),
            Role = "business",
            ProfileId = business.Id
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return (new AuthResponse(GenerateToken(user, business.Name), "business", business.Id, business.Name, business.Industry), null);
    }

    public async Task<(AuthResponse? response, string? error)> RegisterPilotAsync(RegisterPilotRequest req)
    {
        if (await db.Users.AnyAsync(u => u.Email == req.Email))
            return (null, "Email already registered.");

        var pilot = new Pilot
        {
            Name = req.Name,
            Phone = req.Phone,
            VehicleType = req.VehicleType,
            VehiclePlate = "",
            ZonesJson = JsonSerializer.Serialize(new[] { req.City }),
            Rating = 0,
            CompletedDeliveries = 0,
            ReliabilityScore = 0,
            IsPreferred = false,
            IsOnline = false,
            VerificationStatus = "pending",
            PerKmRate = 0,
            BaseFee = 0
        };
        db.Pilots.Add(pilot);
        await db.SaveChangesAsync();

        var user = new User
        {
            Email = req.Email,
            PasswordHash = BCrypt.HashPassword(req.Password),
            Role = "pilot",
            ProfileId = pilot.Id
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return (new AuthResponse(GenerateToken(user, pilot.Name), "pilot", pilot.Id, pilot.Name, null), null);
    }

    public async Task<(AuthResponse? response, string? error)> LoginAsync(LoginRequest req)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
        if (user is null || !BCrypt.Verify(req.Password, user.PasswordHash))
            return (null, "Invalid email or password.");

        string name = "";
        string? industry = null;
        if (user.Role == "business")
        {
            var biz = await db.Businesses.FindAsync(user.ProfileId);
            name = biz?.Name ?? "";
            industry = biz?.Industry;
        }
        else
        {
            name = (await db.Pilots.FindAsync(user.ProfileId))?.Name ?? "";
        }

        return (new AuthResponse(GenerateToken(user, name), user.Role, user.ProfileId, name, industry), null);
    }

    private string GenerateToken(User user, string name)
    {
        var secret = config["Jwt:Secret"] ?? "parcelpilot-dev-secret-key-change-in-prod";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim("role", user.Role),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("profileId", user.ProfileId.ToString()),
            new Claim("name", name),
            new Claim(ClaimTypes.Name, name),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: "parcelpilot",
            audience: "parcelpilot",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(30),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static class BCrypt
    {
        public static string HashPassword(string password) =>
            Convert.ToBase64String(
                System.Security.Cryptography.SHA256.HashData(
                    Encoding.UTF8.GetBytes(password + "parcelpilot-salt")
                )
            );

        public static bool Verify(string password, string hash) =>
            HashPassword(password) == hash;
    }
}
