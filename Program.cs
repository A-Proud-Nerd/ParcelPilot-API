using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDb>(opt => opt.UseSqlite("Data Source=parcelpilot.db"));
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new() { Title = "ParcelPilot API", Version = "v1" }));

var app = builder.Build();

app.UseCors();
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ParcelPilot API v1"));

// Seed and migrate
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDb>();
    db.Database.EnsureCreated();

    if (!db.Businesses.Any())
        db.Businesses.Add(new Business { Id = 1, Name = "Acme Supplies" });

    if (!db.Pilots.Any())
        db.Pilots.AddRange(
            new Pilot { Id = 1, Name = "Sipho Dlamini", Vehicle = "Motorbike", Zone = "Johannesburg North" },
            new Pilot { Id = 2, Name = "Lerato Mokoena", Vehicle = "Sedan", Zone = "Sandton / Rosebank" },
            new Pilot { Id = 3, Name = "Thabo Nkosi", Vehicle = "Bakkie", Zone = "Soweto / South" }
        );

    db.SaveChanges();
}

// --- Deliveries ---

app.MapGet("/deliveries", async (AppDb db) =>
    await db.Deliveries
        .Include(d => d.Quotes)
        .OrderByDescending(d => d.CreatedAt)
        .ToListAsync());

app.MapGet("/deliveries/{id}", async (int id, AppDb db) =>
    await db.Deliveries.Include(d => d.Quotes).FirstOrDefaultAsync(d => d.Id == id)
        is Delivery d ? Results.Ok(d) : Results.NotFound());

app.MapPost("/deliveries", async (CreateDeliveryRequest req, AppDb db) =>
{
    var delivery = new Delivery
    {
        BusinessId = 1,
        PickupAddress = req.PickupAddress,
        DropoffAddress = req.DropoffAddress,
        Description = req.Description,
        Status = "Pending",
        CreatedAt = DateTime.UtcNow
    };
    db.Deliveries.Add(delivery);
    await db.SaveChangesAsync();
    return Results.Created($"/deliveries/{delivery.Id}", delivery);
});

app.MapPut("/deliveries/{id}/status", async (int id, UpdateStatusRequest req, AppDb db) =>
{
    var delivery = await db.Deliveries.FindAsync(id);
    if (delivery is null) return Results.NotFound();
    delivery.Status = req.Status;
    await db.SaveChangesAsync();
    return Results.Ok(delivery);
});

// --- Quotes ---

app.MapPost("/deliveries/{id}/quotes", async (int id, SubmitQuoteRequest req, AppDb db) =>
{
    var delivery = await db.Deliveries.FindAsync(id);
    if (delivery is null) return Results.NotFound();
    if (delivery.Status != "Pending") return Results.BadRequest("Delivery is not open for quotes.");

    var quote = new Quote
    {
        DeliveryId = id,
        PilotId = req.PilotId,
        Amount = req.Amount,
        Note = req.Note,
        Status = "Pending",
        SubmittedAt = DateTime.UtcNow
    };
    db.Quotes.Add(quote);
    await db.SaveChangesAsync();
    return Results.Created($"/deliveries/{id}/quotes/{quote.Id}", quote);
});

app.MapPut("/deliveries/{id}/quotes/{qid}/accept", async (int id, int qid, AppDb db) =>
{
    var delivery = await db.Deliveries.Include(d => d.Quotes).FirstOrDefaultAsync(d => d.Id == id);
    if (delivery is null) return Results.NotFound();

    var quote = delivery.Quotes.FirstOrDefault(q => q.Id == qid);
    if (quote is null) return Results.NotFound();

    foreach (var q in delivery.Quotes) q.Status = q.Id == qid ? "Accepted" : "Rejected";
    delivery.Status = "Assigned";
    delivery.AssignedPilotId = quote.PilotId;

    await db.SaveChangesAsync();
    return Results.Ok(delivery);
});

// --- Reference data ---

app.MapGet("/pilots", async (AppDb db) => await db.Pilots.ToListAsync());
app.MapGet("/businesses", async (AppDb db) => await db.Businesses.ToListAsync());

app.Run();

// --- Models ---

class AppDb(DbContextOptions<AppDb> options) : DbContext(options)
{
    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<Pilot> Pilots => Set<Pilot>();
    public DbSet<Delivery> Deliveries => Set<Delivery>();
    public DbSet<Quote> Quotes => Set<Quote>();
}

class Business { public int Id { get; set; } public string Name { get; set; } = ""; }
class Pilot { public int Id { get; set; } public string Name { get; set; } = ""; public string Vehicle { get; set; } = ""; public string Zone { get; set; } = ""; }

class Delivery
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public string PickupAddress { get; set; } = "";
    public string DropoffAddress { get; set; } = "";
    public string Description { get; set; } = "";
    public string Status { get; set; } = "Pending";
    public int? AssignedPilotId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<Quote> Quotes { get; set; } = [];
}

class Quote
{
    public int Id { get; set; }
    public int DeliveryId { get; set; }
    public int PilotId { get; set; }
    public decimal Amount { get; set; }
    public string? Note { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime SubmittedAt { get; set; }
}

record CreateDeliveryRequest(string PickupAddress, string DropoffAddress, string Description);
record SubmitQuoteRequest(int PilotId, decimal Amount, string? Note);
record UpdateStatusRequest(string Status);
