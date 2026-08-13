using ParcelPilot.Api.DTOs;
using ParcelPilot.Api.Repositories;

namespace ParcelPilot.Api.Endpoints;

public static class PilotEndpoints
{
    public static void MapPilotEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/pilots", async (IPilotRepository repo) =>
            await repo.GetAllAsync());

        app.MapGet("/pilots/{id}", async (Guid id, IPilotRepository repo) =>
        {
            var pilot = await repo.GetByIdAsync(id);
            return pilot is null ? Results.NotFound() : Results.Ok(pilot);
        });

        app.MapPut("/pilots/me", async (HttpContext ctx, UpdatePilotProfileRequest req, IPilotRepository repo) =>
        {
            var user = ctx.User;
            var role = user?.FindFirst("role")?.Value ?? user?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if (user?.Identity?.IsAuthenticated != true || role != "pilot")
                return Results.Forbid();

            var pilotId = Guid.Parse(user.FindFirst("profileId")?.Value ?? Guid.Empty.ToString());
            try
            {
                var updated = await repo.UpdateProfileAsync(pilotId, req);
                return updated is null ? Results.NotFound() : Results.Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });
    }
}
