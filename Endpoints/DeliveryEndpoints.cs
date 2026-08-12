using ParcelPilot.Api.DTOs;
using ParcelPilot.Api.Models;
using ParcelPilot.Api.Repositories;

namespace ParcelPilot.Api.Endpoints;

public static class DeliveryEndpoints
{
    public static void MapDeliveryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/deliveries", async (IDeliveryRepository repo) =>
            await repo.GetAllAsync());

        app.MapGet("/deliveries/{id}", async (int id, IDeliveryRepository repo) =>
        {
            var delivery = await repo.GetByIdAsync(id);
            return delivery is null ? Results.NotFound() : Results.Ok(delivery);
        });

        app.MapPost("/deliveries", async (CreateDeliveryRequest req, IDeliveryRepository repo) =>
        {
            var delivery = new Delivery
            {
                BusinessId = 1,
                Reference = $"PP-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 100000:D5}",
                Description = req.Description,
                WeightKg = req.WeightKg,
                Priority = req.Priority ?? "standard",
                RequiresSignature = req.RequiresSignature,
                Currency = "ZAR",
                Status = "awaiting_approval",
                CreatedAt = DateTime.UtcNow,
                Stops =
                [
                    new DeliveryStop
                    {
                        Sequence = 1, Type = "pickup",
                        ContactName = req.PickupContactName ?? "Sender",
                        ContactPhone = req.PickupContactPhone ?? "",
                        AddressLine1 = req.PickupAddress,
                        City = req.PickupCity ?? "",
                        Status = "pending"
                    },
                    new DeliveryStop
                    {
                        Sequence = 2, Type = "dropoff",
                        ContactName = req.DropoffContactName ?? "Recipient",
                        ContactPhone = req.DropoffContactPhone ?? "",
                        AddressLine1 = req.DropoffAddress,
                        City = req.DropoffCity ?? "",
                        Status = "pending"
                    }
                ]
            };

            var created = await repo.CreateAsync(delivery);
            return Results.Created($"/deliveries/{created.Id}", created);
        });

        app.MapPut("/deliveries/{id}/status", async (int id, UpdateStatusRequest req, IDeliveryRepository repo) =>
        {
            var delivery = await repo.UpdateStatusAsync(id, req.Status);
            return delivery is null ? Results.NotFound() : Results.Ok(delivery);
        });

        app.MapPost("/deliveries/{id}/quotes", async (int id, SubmitQuoteRequest req, IPilotRepository pilotRepo, IDeliveryRepository repo) =>
        {
            var pilot = await pilotRepo.GetByIdAsync(req.PilotId);
            var quote = new Quote
            {
                DeliveryId = id,
                PilotId = req.PilotId,
                PilotName = pilot?.Name ?? $"Pilot #{req.PilotId}",
                PilotRating = pilot?.Rating ?? 0,
                BaseAmount = req.BaseAmount,
                DistanceAmount = req.DistanceAmount,
                Subtotal = req.BaseAmount + req.DistanceAmount,
                Note = req.Note,
                Status = "proposed",
                SubmittedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(req.ExpiresInMinutes ?? 15)
            };

            var created = await repo.AddQuoteAsync(id, quote);
            return created is null
                ? Results.BadRequest("Delivery not found or not open for quotes.")
                : Results.Created($"/deliveries/{id}/quotes/{created.Id}", created);
        });

        app.MapPut("/deliveries/{id}/quotes/{qid}/accept", async (int id, int qid, IDeliveryRepository repo) =>
        {
            var delivery = await repo.AcceptQuoteAsync(id, qid);
            return delivery is null ? Results.NotFound() : Results.Ok(delivery);
        });
    }
}
