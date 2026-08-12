using Microsoft.EntityFrameworkCore;
using ParcelPilot.Api.Models;

namespace ParcelPilot.Api.Data;

public class AppDb(DbContextOptions<AppDb> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<Pilot> Pilots => Set<Pilot>();
    public DbSet<Delivery> Deliveries => Set<Delivery>();
    public DbSet<DeliveryStop> DeliveryStops => Set<DeliveryStop>();
    public DbSet<Quote> Quotes => Set<Quote>();
}
