using ParcelPilot.Api.Repositories;

namespace ParcelPilot.Api.Endpoints;

public static class BusinessEndpoints
{
    public static void MapBusinessEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/businesses", async (IBusinessRepository repo) =>
            await repo.GetAllAsync());

        app.MapPut("/businesses/team/{pilotId}", async (HttpContext ctx, Guid pilotId, IPilotRepository repo) =>
        {
            var user = ctx.User;
            var role = user?.FindFirst("role")?.Value ?? user?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if (user?.Identity?.IsAuthenticated != true || role != "business")
                return Results.Forbid();

            var updated = await repo.TogglePreferredAsync(pilotId, true);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        });
    }
}
