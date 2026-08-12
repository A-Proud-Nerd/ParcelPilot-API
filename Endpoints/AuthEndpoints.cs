using ParcelPilot.Api.DTOs;
using ParcelPilot.Api.Repositories;

namespace ParcelPilot.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/register/business", async (RegisterBusinessRequest req, IAuthRepository repo) =>
        {
            var (response, error) = await repo.RegisterBusinessAsync(req);
            return error is not null
                ? Results.BadRequest(new { error })
                : Results.Ok(response);
        });

        app.MapPost("/auth/register/pilot", async (RegisterPilotRequest req, IAuthRepository repo) =>
        {
            var (response, error) = await repo.RegisterPilotAsync(req);
            return error is not null
                ? Results.BadRequest(new { error })
                : Results.Ok(response);
        });

        app.MapPost("/auth/login", async (LoginRequest req, IAuthRepository repo) =>
        {
            var (response, error) = await repo.LoginAsync(req);
            return error is not null
                ? Results.Unauthorized()
                : Results.Ok(response);
        });
    }
}
