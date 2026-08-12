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
    }
}
