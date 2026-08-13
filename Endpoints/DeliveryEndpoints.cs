using ParcelPilot.Api.DTOs;
using ParcelPilot.Api.Models;
using ParcelPilot.Api.Repositories;

namespace ParcelPilot.Api.Endpoints;

public static class DeliveryEndpoints
{
    public static void MapDeliveryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/deliveries", async (HttpContext ctx, IDeliveryRepository repo) =>
        {
            var deliveries = await repo.GetAllAsync();
            var user = ctx.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var role = user.FindFirst("role")?.Value ?? user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                var pidClaim = user.FindFirst("profileId")?.Value;
                if (role == "pilot" && Guid.TryParse(pidClaim, out var pilotId))
                {
                    var visible = deliveries
                        .Where(d => d.AssignedPilotId == pilotId || d.RequestedPilotId == pilotId)
                        .ToList();
                    return Results.Ok(visible);
                }

                if (role == "business" && Guid.TryParse(pidClaim, out var businessId))
                {
                    var visible = deliveries.Where(d => d.BusinessId == businessId).ToList();
                    return Results.Ok(visible);
                }
            }

            // Unauthenticated or other roles: only show public deliveries
            var publics = deliveries.Where(d => d.IsPublic).ToList();
            return Results.Ok(publics);
        });

        app.MapGet("/deliveries/{id}", async (Guid id, IDeliveryRepository repo) =>
        {
            var delivery = await repo.GetByIdAsync(id);
            return delivery is null ? Results.NotFound() : Results.Ok(delivery);
        });

        app.MapPost("/deliveries", async (HttpContext ctx, CreateDeliveryRequest req, IDeliveryRepository repo) =>
        {
            var user = ctx.User;
            var role = user?.FindFirst("role")?.Value ?? user?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if (user?.Identity?.IsAuthenticated != true || role != "business")
                return Results.Forbid();

            var profileId = Guid.Parse(user.FindFirst("profileId")?.Value ?? Guid.Empty.ToString());

            var delivery = new Delivery
            {
                BusinessId = profileId,
                Reference = $"PP-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 100000:D5}",
                Description = req.Description,
                WeightKg = req.WeightKg,
                Priority = req.Priority ?? "standard",
                RequiresSignature = req.RequiresSignature,
                Currency = "ZAR",
                IsPublic = req.IsPublic,
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

            // Request-first matching flow: direct pilot requests create a private, targeted request.
            if (req.PilotId.HasValue)
            {
                delivery.RequestedPilotId = req.PilotId.Value;
                delivery.RequestedAt = DateTime.UtcNow;
                delivery.Status = "awaiting_pilot_response";
            }
            else if (req.IsPublic)
            {
                delivery.Status = "awaiting_approval";
            }
            else
            {
                delivery.Status = "awaiting_pilot";
            }

            var created = await repo.CreateAsync(delivery);
            return Results.Created($"/deliveries/{created.Id}", created);
        });

        app.MapPut("/deliveries/{id}/status", async (Guid id, UpdateStatusRequest req, IDeliveryRepository repo) =>
        {
            var delivery = await repo.UpdateStatusAsync(id, req.Status);
            return delivery is null ? Results.NotFound() : Results.Ok(delivery);
        });

        app.MapPut("/deliveries/{id}/request-pilot", async (HttpContext ctx, Guid id, AssignPilotRequest req, IDeliveryRepository repo, IPilotRepository pilotRepo) =>
        {
            var user = ctx.User;
            var role = user?.FindFirst("role")?.Value ?? user?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if (user?.Identity?.IsAuthenticated != true || role != "business")
                return Results.Forbid();

            var profileId = Guid.Parse(user.FindFirst("profileId")?.Value ?? Guid.Empty.ToString());
            var delivery = await repo.GetByIdAsync(id);
            if (delivery is null || delivery.BusinessId != profileId)
                return Results.Forbid();

            var pilot = await pilotRepo.GetByIdAsync(req.PilotId);
            if (pilot is null)
                return Results.NotFound();

            var updated = await repo.RequestPilotAsync(id, req.PilotId);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        });

        app.MapPut("/deliveries/{id}/respond-request", async (HttpContext ctx, Guid id, RespondToRequestRequest req, IDeliveryRepository repo) =>
        {
            var user = ctx.User;
            var role = user?.FindFirst("role")?.Value ?? user?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if (user?.Identity?.IsAuthenticated != true || role != "pilot")
                return Results.Forbid();

            var pilotId = Guid.Parse(user.FindFirst("profileId")?.Value ?? Guid.Empty.ToString());
            var delivery = await repo.GetByIdAsync(id);
            if (delivery is null || delivery.RequestedPilotId != pilotId)
                return Results.Forbid();

            var updated = req.Accept
                ? await repo.AcceptRequestAsync(id, pilotId)
                : await repo.DeclineRequestAsync(id, pilotId);

            return updated is null ? Results.NotFound() : Results.Ok(updated);
        });

        app.MapPut("/deliveries/{id}/assign", async (HttpContext ctx, Guid id, AssignPilotRequest req, IDeliveryRepository repo, IPilotRepository pilotRepo) =>
        {
            var user = ctx.User;
            var role = user?.FindFirst("role")?.Value ?? user?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if (user?.Identity?.IsAuthenticated != true || role != "business")
                return Results.Forbid();

            var profileId = Guid.Parse(user.FindFirst("profileId")?.Value ?? Guid.Empty.ToString());
            var delivery = await repo.GetByIdAsync(id);
            if (delivery is null || delivery.BusinessId != profileId)
                return Results.Forbid();

            var pilot = await pilotRepo.GetByIdAsync(req.PilotId);
            if (pilot is null)
                return Results.NotFound();

            var updated = await repo.AssignPilotAsync(id, req.PilotId);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        });

        app.MapPost("/deliveries/{id}/quotes", async (HttpContext ctx, Guid id, SubmitQuoteRequest req, IPilotRepository pilotRepo, IDeliveryRepository repo) =>
        {
            var user = ctx.User;
            var role = user?.FindFirst("role")?.Value ?? user?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if (user?.Identity?.IsAuthenticated != true || role != "pilot")
                return Results.Forbid();

            var profileId = Guid.Parse(user.FindFirst("profileId")?.Value ?? Guid.Empty.ToString());
            if (profileId != req.PilotId)
                return Results.BadRequest("Pilot id mismatch.");

            var delivery = await repo.GetByIdAsync(id);
            var isEligibleForNegotiation = delivery != null && (delivery.IsPublic || delivery.RequestedPilotId == req.PilotId);
            var isOpenStatus = delivery != null && (delivery.Status == "awaiting_approval" || delivery.Status == "awaiting_pilot_response");
            if (!isEligibleForNegotiation || !isOpenStatus)
                return Results.BadRequest("Delivery not open for quotes.");

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

        app.MapPut("/deliveries/{id}/quotes/{qid}/accept", async (HttpContext ctx, Guid id, Guid qid, IDeliveryRepository repo) =>
        {
            var user = ctx.User;
            var role = user?.FindFirst("role")?.Value ?? user?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if (user?.Identity?.IsAuthenticated != true || role != "business")
                return Results.Forbid();

            var profileId = Guid.Parse(user.FindFirst("profileId")?.Value ?? Guid.Empty.ToString());
            var delivery = await repo.GetByIdAsync(id);
            if (delivery == null || delivery.BusinessId != profileId)
                return Results.Forbid();

            var updated = await repo.AcceptQuoteAsync(id, qid);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        });

        app.MapPut("/deliveries/{id}/quotes/{qid}/decline", async (HttpContext ctx, Guid id, Guid qid, IDeliveryRepository repo) =>
        {
            var user = ctx.User;
            var role = user?.FindFirst("role")?.Value ?? user?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if (user?.Identity?.IsAuthenticated != true || role != "business")
                return Results.Forbid();

            var profileId = Guid.Parse(user.FindFirst("profileId")?.Value ?? Guid.Empty.ToString());
            var delivery = await repo.GetByIdAsync(id);
            if (delivery == null || delivery.BusinessId != profileId)
                return Results.Forbid();

            var updated = await repo.RejectQuoteAsync(id, qid);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        });
    }
}
