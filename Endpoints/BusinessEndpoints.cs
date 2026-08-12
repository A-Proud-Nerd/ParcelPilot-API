using ParcelPilot.Api.Repositories;

namespace ParcelPilot.Api.Endpoints;

public static class BusinessEndpoints
{
    public static void MapBusinessEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/businesses", async (IBusinessRepository repo) =>
            await repo.GetAllAsync());
    }
}
