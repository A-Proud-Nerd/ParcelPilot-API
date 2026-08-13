using Microsoft.EntityFrameworkCore;
using ParcelPilot.Api.Data;
using ParcelPilot.Api.Models;

namespace ParcelPilot.Api.Repositories;

public class DeliveryRepository(AppDb db) : IDeliveryRepository
{
    public Task<List<Delivery>> GetAllAsync() =>
        db.Deliveries
            .Include(d => d.Quotes)
            .Include(d => d.Stops)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

    public Task<Delivery?> GetByIdAsync(Guid id) =>
        db.Deliveries
            .Include(d => d.Quotes)
            .Include(d => d.Stops)
            .FirstOrDefaultAsync(d => d.Id == id);

    public async Task<Delivery> CreateAsync(Delivery delivery)
    {
        db.Deliveries.Add(delivery);
        await db.SaveChangesAsync();
        await db.Entry(delivery).Collection(d => d.Stops).LoadAsync();
        return delivery;
    }

    public async Task<Delivery?> UpdateStatusAsync(Guid id, string status)
    {
        var delivery = await db.Deliveries
            .Include(d => d.Stops)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (delivery is null) return null;

        delivery.Status = status;

        if (status == "in_transit")
            foreach (var s in delivery.Stops.Where(s => s.Type == "pickup"))
                s.Status = "completed";

        if (status == "completed")
            foreach (var s in delivery.Stops)
                s.Status = "completed";

        await db.SaveChangesAsync();
        await db.Entry(delivery).Collection(d => d.Quotes).LoadAsync();
        return delivery;
    }

    public async Task<Delivery?> AssignPilotAsync(Guid deliveryId, Guid pilotId)
    {
        var delivery = await db.Deliveries
            .Include(d => d.Stops)
            .Include(d => d.Quotes)
            .FirstOrDefaultAsync(d => d.Id == deliveryId);

        if (delivery is null) return null;

        delivery.AssignedPilotId = pilotId;
        delivery.RequestedPilotId = null;
        delivery.RequestedAt = null;
        delivery.Status = "confirmed";
        delivery.IsPublic = false;

        await db.SaveChangesAsync();
        return delivery;
    }

    public async Task<Delivery?> RequestPilotAsync(Guid deliveryId, Guid pilotId)
    {
        var delivery = await db.Deliveries
            .Include(d => d.Stops)
            .Include(d => d.Quotes)
            .FirstOrDefaultAsync(d => d.Id == deliveryId);

        if (delivery is null) return null;

        delivery.RequestedPilotId = pilotId;
        delivery.RequestedAt = DateTime.UtcNow;
        delivery.AssignedPilotId = null;
        delivery.Status = "awaiting_pilot_response";
        delivery.IsPublic = false;

        await db.SaveChangesAsync();
        return delivery;
    }

    public async Task<Delivery?> AcceptRequestAsync(Guid deliveryId, Guid pilotId)
    {
        var delivery = await db.Deliveries
            .Include(d => d.Stops)
            .Include(d => d.Quotes)
            .FirstOrDefaultAsync(d => d.Id == deliveryId);

        if (delivery is null || delivery.RequestedPilotId != pilotId)
            return null;

        delivery.AssignedPilotId = pilotId;
        delivery.RequestedPilotId = null;
        delivery.RequestedAt = null;
        delivery.Status = "confirmed";
        delivery.IsPublic = false;

        await db.SaveChangesAsync();
        return delivery;
    }

    public async Task<Delivery?> DeclineRequestAsync(Guid deliveryId, Guid pilotId)
    {
        var delivery = await db.Deliveries
            .Include(d => d.Stops)
            .Include(d => d.Quotes)
            .FirstOrDefaultAsync(d => d.Id == deliveryId);

        if (delivery is null || delivery.RequestedPilotId != pilotId)
            return null;

        delivery.RequestedPilotId = null;
        delivery.RequestedAt = null;
        delivery.Status = "awaiting_pilot";
        delivery.IsPublic = false;

        await db.SaveChangesAsync();
        return delivery;
    }

    public async Task<Quote?> AddQuoteAsync(Guid deliveryId, Quote quote)
    {
        var delivery = await db.Deliveries.FindAsync(deliveryId);
        if (delivery is null || delivery.Status != "awaiting_approval") return null;

        db.Quotes.Add(quote);
        await db.SaveChangesAsync();
        return quote;
    }

    public async Task<Delivery?> AcceptQuoteAsync(Guid deliveryId, Guid quoteId)
    {
        var delivery = await db.Deliveries
            .Include(d => d.Quotes)
            .Include(d => d.Stops)
            .FirstOrDefaultAsync(d => d.Id == deliveryId);

        if (delivery is null) return null;

        var quote = delivery.Quotes.FirstOrDefault(q => q.Id == quoteId);
        if (quote is null) return null;

        foreach (var q in delivery.Quotes)
            q.Status = q.Id == quoteId ? "accepted" : "rejected";

        delivery.Status = "confirmed";
        delivery.AssignedPilotId = quote.PilotId;
        delivery.AgreedSubtotal = quote.Subtotal;
        delivery.ServiceFee = Math.Round(quote.Subtotal * 0.03m, 2);
        delivery.TotalCharge = delivery.AgreedSubtotal + delivery.ServiceFee;

        await db.SaveChangesAsync();
        return delivery;
    }
}
