using Microsoft.EntityFrameworkCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDb>(opt => opt.UseSqlite("Data Source=parcelpilot.db"));
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new() { Title = "ParcelPilot API", Version = "v1" }));

var app = builder.Build();

app.UseCors();
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ParcelPilot API v1"));

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDb>();
    db.Database.EnsureCreated();

    if (!db.Businesses.Any())
        db.Businesses.Add(new Business { Id = 1, Name = "Retail Co", Industry = "Retail & e-commerce", Address = "14 Roeland St, Gardens, Cape Town", VerificationStatus = "verified" });

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

// --- Deliveries ---

app.MapGet("/deliveries", async (AppDb db) =>
    await db.Deliveries
        .Include(d => d.Quotes)
        .Include(d => d.Stops)
        .OrderByDescending(d => d.CreatedAt)
        .ToListAsync());

app.MapGet("/deliveries/{id}", async (int id, AppDb db) =>
{
    var d = await db.Deliveries.Include(d => d.Quotes).Include(d => d.Stops).FirstOrDefaultAsync(d => d.Id == id);
    return d is null ? Results.NotFound() : Results.Ok(d);
});

app.MapPost("/deliveries", async (CreateDeliveryRequest req, AppDb db) =>
{
    var count = await db.Deliveries.CountAsync();
    var delivery = new Delivery
    {
        BusinessId = 1,
        Reference = $"PP-{48200 + count + 1}",
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
    db.Deliveries.Add(delivery);
    await db.SaveChangesAsync();
    await db.Entry(delivery).Collection(d => d.Stops).LoadAsync();
    return Results.Created($"/deliveries/{delivery.Id}", delivery);
});

app.MapPut("/deliveries/{id}/status", async (int id, UpdateStatusRequest req, AppDb db) =>
{
    var delivery = await db.Deliveries.Include(d => d.Stops).FirstOrDefaultAsync(d => d.Id == id);
    if (delivery is null) return Results.NotFound();
    delivery.Status = req.Status;

    if (req.Status == "in_transit")
        foreach (var s in delivery.Stops.Where(s => s.Type == "pickup")) s.Status = "completed";
    if (req.Status == "completed")
        foreach (var s in delivery.Stops) s.Status = "completed";

    await db.SaveChangesAsync();
    await db.Entry(delivery).Collection(d => d.Quotes).LoadAsync();
    return Results.Ok(delivery);
});

// --- Quotes ---

app.MapPost("/deliveries/{id}/quotes", async (int id, SubmitQuoteRequest req, AppDb db) =>
{
    var delivery = await db.Deliveries.FindAsync(id);
    if (delivery is null) return Results.NotFound();
    if (delivery.Status != "awaiting_approval") return Results.BadRequest("Delivery is not open for quotes.");

    var pilot = await db.Pilots.FindAsync(req.PilotId);
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
    db.Quotes.Add(quote);
    await db.SaveChangesAsync();
    return Results.Created($"/deliveries/{id}/quotes/{quote.Id}", quote);
});

app.MapPut("/deliveries/{id}/quotes/{qid}/accept", async (int id, int qid, AppDb db) =>
{
    var delivery = await db.Deliveries.Include(d => d.Quotes).Include(d => d.Stops).FirstOrDefaultAsync(d => d.Id == id);
    if (delivery is null) return Results.NotFound();

    var quote = delivery.Quotes.FirstOrDefault(q => q.Id == qid);
    if (quote is null) return Results.NotFound();

    foreach (var q in delivery.Quotes) q.Status = q.Id == qid ? "accepted" : "rejected";
    delivery.Status = "confirmed";
    delivery.AssignedPilotId = quote.PilotId;
    delivery.AgreedSubtotal = quote.Subtotal;
    delivery.ServiceFee = Math.Round(quote.Subtotal * 0.03m, 2);
    delivery.TotalCharge = delivery.AgreedSubtotal + delivery.ServiceFee;

    await db.SaveChangesAsync();
    return Results.Ok(delivery);
});

// --- Pilots ---

app.MapGet("/pilots", async (AppDb db) =>
    (await db.Pilots.ToListAsync()).Select(p => p.ToDto()));

app.MapGet("/pilots/{id}", async (int id, AppDb db) =>
{
    var p = await db.Pilots.FindAsync(id);
    return p is null ? Results.NotFound() : Results.Ok(p.ToDto());
});

// --- Businesses ---

app.MapGet("/businesses", async (AppDb db) => await db.Businesses.ToListAsync());

app.Run();

// --- Models ---

class AppDb(DbContextOptions<AppDb> options) : DbContext(options)
{
    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<Pilot> Pilots => Set<Pilot>();
    public DbSet<Delivery> Deliveries => Set<Delivery>();
    public DbSet<DeliveryStop> DeliveryStops => Set<DeliveryStop>();
    public DbSet<Quote> Quotes => Set<Quote>();
}

class Business
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Industry { get; set; } = "";
    public string Address { get; set; } = "";
    public string VerificationStatus { get; set; } = "pending";
}

class Pilot
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Rating { get; set; }
    public int CompletedDeliveries { get; set; }
    public int ReliabilityScore { get; set; }
    public string VehicleType { get; set; } = "";
    public string VehiclePlate { get; set; } = "";
    public string ZonesJson { get; set; } = "[]";
    public bool IsPreferred { get; set; }
    public bool IsOnline { get; set; }
    public string VerificationStatus { get; set; } = "pending";
    public decimal PerKmRate { get; set; }
    public decimal BaseFee { get; set; }
    public string Phone { get; set; } = "";

    public PilotDto ToDto() => new(
        Id, Name, Rating, CompletedDeliveries, ReliabilityScore,
        VehicleType, VehiclePlate,
        JsonSerializer.Deserialize<string[]>(ZonesJson) ?? [],
        IsPreferred, IsOnline, VerificationStatus, PerKmRate, BaseFee, Phone
    );
}

record PilotDto(
    int Id, string Name, decimal Rating, int CompletedDeliveries, int ReliabilityScore,
    string VehicleType, string VehiclePlate, string[] Zones,
    bool IsPreferred, bool IsOnline, string VerificationStatus,
    decimal PerKmRate, decimal BaseFee, string Phone
);

class Delivery
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public string Reference { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal WeightKg { get; set; }
    public string Priority { get; set; } = "standard";
    public bool RequiresSignature { get; set; }
    public string Currency { get; set; } = "ZAR";
    public string Status { get; set; } = "awaiting_approval";
    public int? AssignedPilotId { get; set; }
    public decimal AgreedSubtotal { get; set; }
    public decimal ServiceFee { get; set; }
    public decimal TotalCharge { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ScheduledFor { get; set; }
    public List<DeliveryStop> Stops { get; set; } = [];
    public List<Quote> Quotes { get; set; } = [];
}

class DeliveryStop
{
    public int Id { get; set; }
    public int DeliveryId { get; set; }
    public int Sequence { get; set; }
    public string Type { get; set; } = "";
    public string ContactName { get; set; } = "";
    public string ContactPhone { get; set; } = "";
    public string AddressLine1 { get; set; } = "";
    public string City { get; set; } = "";
    public string? Instructions { get; set; }
    public string Status { get; set; } = "pending";
}

class Quote
{
    public int Id { get; set; }
    public int DeliveryId { get; set; }
    public int PilotId { get; set; }
    public string PilotName { get; set; } = "";
    public decimal PilotRating { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal DistanceAmount { get; set; }
    public decimal Subtotal { get; set; }
    public string? Note { get; set; }
    public string Status { get; set; } = "proposed";
    public DateTime SubmittedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

record CreateDeliveryRequest(
    string PickupAddress, string DropoffAddress, string Description,
    string? PickupContactName, string? PickupContactPhone, string? PickupCity,
    string? DropoffContactName, string? DropoffContactPhone, string? DropoffCity,
    decimal WeightKg, string? Priority, bool RequiresSignature
);
record SubmitQuoteRequest(int PilotId, decimal BaseAmount, decimal DistanceAmount, string? Note, int? ExpiresInMinutes);
record UpdateStatusRequest(string Status);
