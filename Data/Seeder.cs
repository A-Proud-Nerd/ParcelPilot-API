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
                Id = 1,
                Name = "Retail Co",
                Industry = "Retail & e-commerce",
                Address = "14 Roeland St, Gardens, Cape Town",
                VerificationStatus = "verified"
            });

        if (!db.Pilots.Any())
            db.Pilots.AddRange(
                new Pilot
                {
                    Id = 1, Name = "Thabo Nkosi", Rating = 4.9m, CompletedDeliveries = 412, ReliabilityScore = 98,
                    VehicleType = "van", VehiclePlate = "CA 442-198",
                    ZonesJson = JsonSerializer.Serialize(new[] { "Woodstock", "Salt River", "CBD" }),
                    IsPreferred = true, IsOnline = true, VerificationStatus = "verified",
                    PerKmRate = 6.5m, BaseFee = 45m, Phone = "+27 82 555 0142"
                },
                new Pilot
                {
                    Id = 2, Name = "Lerato Dube", Rating = 4.8m, CompletedDeliveries = 289, ReliabilityScore = 95,
                    VehicleType = "sedan", VehiclePlate = "CA 991-322",
                    ZonesJson = JsonSerializer.Serialize(new[] { "Observatory", "Rondebosch" }),
                    IsPreferred = true, IsOnline = true, VerificationStatus = "verified",
                    PerKmRate = 5.8m, BaseFee = 40m, Phone = "+27 83 555 0198"
                },
                new Pilot
                {
                    Id = 3, Name = "Sipho Mahlangu", Rating = 4.6m, CompletedDeliveries = 156, ReliabilityScore = 91,
                    VehicleType = "bike", VehiclePlate = "N/A",
                    ZonesJson = JsonSerializer.Serialize(new[] { "CBD", "Gardens" }),
                    IsPreferred = false, IsOnline = false, VerificationStatus = "verified",
                    PerKmRate = 4.2m, BaseFee = 25m, Phone = "+27 84 555 0173"
                },
                new Pilot
                {
                    Id = 4, Name = "Nomvula Zulu", Rating = 5.0m, CompletedDeliveries = 67, ReliabilityScore = 99,
                    VehicleType = "truck", VehiclePlate = "CA 118-873",
                    ZonesJson = JsonSerializer.Serialize(new[] { "Epping", "Parow", "Bellville" }),
                    IsPreferred = false, IsOnline = true, VerificationStatus = "pending",
                    PerKmRate = 8.9m, BaseFee = 90m, Phone = "+27 71 555 0261"
                }
            );

        db.SaveChanges();
    }
}
