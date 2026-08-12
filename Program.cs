using Microsoft.EntityFrameworkCore;
using ParcelPilot.Api.Data;
using ParcelPilot.Api.Endpoints;
using ParcelPilot.Api.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDb>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=parcelpilot.db"));

builder.Services.AddScoped<IDeliveryRepository, DeliveryRepository>();
builder.Services.AddScoped<IPilotRepository, PilotRepository>();
builder.Services.AddScoped<IBusinessRepository, BusinessRepository>();

builder.Services.AddCors(o =>
    o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
    c.SwaggerDoc("v1", new() { Title = "ParcelPilot API", Version = "v1" }));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
    Seeder.Seed(scope.ServiceProvider.GetRequiredService<AppDb>());

app.UseCors();
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ParcelPilot API v1"));

app.MapDeliveryEndpoints();
app.MapPilotEndpoints();
app.MapBusinessEndpoints();

app.Run();
