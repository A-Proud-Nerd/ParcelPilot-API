using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ParcelPilot.Api.Data;
using ParcelPilot.Api.Endpoints;
using ParcelPilot.Api.Realtime;
using ParcelPilot.Api.Repositories;

var builder = WebApplication.CreateBuilder(args);

var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (string.IsNullOrWhiteSpace(databaseUrl))
{
    throw new InvalidOperationException("DATABASE_URL is required. Configure the PostgreSQL connection string in Railway before deployment.");
}

if (!Uri.TryCreate(databaseUrl, UriKind.Absolute, out var uri) || !uri.Scheme.StartsWith("postgres", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException($"DATABASE_URL must be a PostgreSQL connection string. Received: {databaseUrl}");
}

var userInfo = uri.UserInfo.Split(':', 2);
var connectionStringBuilder = new Npgsql.NpgsqlConnectionStringBuilder
{
    Host = uri.Host,
    Port = uri.Port,
    Username = userInfo.Length > 0 ? userInfo[0] : string.Empty,
    Password = userInfo.Length > 1 ? userInfo[1] : string.Empty,
    Database = uri.AbsolutePath.TrimStart('/'),
    SslMode = Npgsql.SslMode.Require,
    Pooling = true
};

builder.Services.AddDbContext<AppDb>(opt => opt.UseNpgsql(connectionStringBuilder.ToString()));

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IDeliveryRepository, DeliveryRepository>();
builder.Services.AddScoped<IPilotRepository, PilotRepository>();
builder.Services.AddScoped<IBusinessRepository, BusinessRepository>();

var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "parcelpilot-dev-secret-key-change-in-prod";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.MapInboundClaims = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "parcelpilot",
            ValidAudience = "parcelpilot",
            RoleClaimType = "role",
            NameClaimType = "name",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddCors(o =>
    o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
    c.SwaggerDoc("v1", new() { Title = "ParcelPilot API", Version = "v1" }));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDb>();
    try
    {
        // If migrations are present, apply them. Otherwise fall back to EnsureCreated for initial schema.
        if (db.Database.GetMigrations().Any())
            db.Database.Migrate();
        else
            db.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database migration error: {ex.Message}");
    }

    Seeder.Seed(db);
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ParcelPilot API v1"));

app.MapAuthEndpoints();
app.MapDeliveryEndpoints();
app.MapPilotEndpoints();
app.MapBusinessEndpoints();

app.MapGet("/delivery-events", async (HttpContext context) =>
{
    context.Response.Headers.Append("Cache-Control", "no-cache");
    context.Response.Headers.Append("Connection", "keep-alive");
    context.Response.ContentType = "text/event-stream";

    var subscriberId = DeliveryEventHub.Subscribe();
    context.Response.OnCompleted(() =>
    {
        DeliveryEventHub.Unsubscribe(subscriberId);
        return Task.CompletedTask;
    });

    var reader = DeliveryEventHub.GetReader(subscriberId);
    if (reader is null)
    {
        return;
    }

    await foreach (var payload in reader.ReadAllAsync(context.RequestAborted))
    {
        await context.Response.WriteAsync($"data: {payload}\n\n");
        await context.Response.Body.FlushAsync(context.RequestAborted);
    }
});

app.Run();
