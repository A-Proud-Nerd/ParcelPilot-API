using System.Text.Json;
using ParcelPilot.Api.Models;

namespace ParcelPilot.Api.Data;

public static class Seeder
{
    public static void Seed(AppDb db)
    {
        // Assumes migrations have been applied. Seed only when empty to avoid overwriting data.
        if (!db.Businesses.Any())
            db.Businesses.Add(new Business
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Retail Co",
                Industry = "Retail & e-commerce",
                Address = "14 Roeland St, Gardens, Cape Town",
                VerificationStatus = "verified"
            });

        if (!db.Pilots.Any())
            db.Pilots.AddRange(
                new Pilot
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Thabo Nkosi", Rating = 4.9m, CompletedDeliveries = 412, ReliabilityScore = 98,
                    VehicleType = "van", VehiclePlate = "CA 442-198",
                    ZonesJson = JsonSerializer.Serialize(new[] { "Woodstock", "Salt River", "CBD" }),
                    IsPreferred = true, IsOnline = true, VerificationStatus = "verified",
                    PerKmRate = 6.5m, BaseFee = 45m, Phone = "+27 82 555 0142"
                },
                new Pilot
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Lerato Dube", Rating = 4.8m, CompletedDeliveries = 289, ReliabilityScore = 95,
                    VehicleType = "sedan", VehiclePlate = "CA 991-322",
                    ZonesJson = JsonSerializer.Serialize(new[] { "Observatory", "Rondebosch" }),
                    IsPreferred = true, IsOnline = true, VerificationStatus = "verified",
                    PerKmRate = 5.8m, BaseFee = 40m, Phone = "+27 83 555 0198"
                },
                new Pilot
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "Sipho Mahlangu", Rating = 4.6m, CompletedDeliveries = 156, ReliabilityScore = 91,
                    VehicleType = "bike", VehiclePlate = "N/A",
                    ZonesJson = JsonSerializer.Serialize(new[] { "CBD", "Gardens" }),
                    IsPreferred = false, IsOnline = false, VerificationStatus = "verified",
                    PerKmRate = 4.2m, BaseFee = 25m, Phone = "+27 84 555 0173"
                },
                new Pilot
                {
                    Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), Name = "Nomvula Zulu", Rating = 5.0m, CompletedDeliveries = 67, ReliabilityScore = 99,
                    VehicleType = "truck", VehiclePlate = "CA 118-873",
                    ZonesJson = JsonSerializer.Serialize(new[] { "Epping", "Parow", "Bellville" }),
                    IsPreferred = false, IsOnline = true, VerificationStatus = "pending",
                    PerKmRate = 8.9m, BaseFee = 90m, Phone = "+27 71 555 0261"
                }
            );

        db.SaveChanges();
    }
}
