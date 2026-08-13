using Microsoft.EntityFrameworkCore;
using ParcelPilot.Api.Data;
using ParcelPilot.Api.Models;

namespace ParcelPilot.Api.Repositories;

public class DeliveryRepository(AppDb db) : IDeliveryRepository
{
    private static void EnsureEstimatedPrice(Delivery delivery, decimal? baseFee = null, decimal? perKmRate = null)
    {
        if (delivery.AgreedSubtotal > 0m)
            return;

        var estimate = baseFee ?? 0m;
        if (perKmRate is not null)
            estimate += perKmRate.Value * Math.Max(delivery.WeightKg, 1m);

        if (estimate <= 0m)
            estimate = delivery.Priority == "express" ? 145m : 96m;

        delivery.AgreedSubtotal = Math.Round(estimate, 2);
        delivery.ServiceFee = Math.Round(delivery.AgreedSubtotal * 0.03m, 2);
        delivery.TotalCharge = delivery.AgreedSubtotal + delivery.ServiceFee;
    }

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
        if (delivery.RequestedPilotId is Guid requestedPilotId)
        {
            var pilot = await db.Pilots.FirstOrDefaultAsync(p => p.Id == requestedPilotId);
            EnsureEstimatedPrice(delivery, pilot?.BaseFee, pilot?.PerKmRate);
        }
        else if (delivery.AgreedSubtotal <= 0m)
        {
            EnsureEstimatedPrice(delivery, delivery.Priority == "express" ? 145m : 96m, null);
        }

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

        var pilot = await db.Pilots.FirstOrDefaultAsync(p => p.Id == pilotId);
        delivery.RequestedPilotId = pilotId;
        delivery.RequestedAt = DateTime.UtcNow;
        delivery.AssignedPilotId = null;
        delivery.Status = "awaiting_pilot_response";
        delivery.IsPublic = false;
        EnsureEstimatedPrice(delivery, pilot?.BaseFee, pilot?.PerKmRate);

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

        var pilot = await db.Pilots.FirstOrDefaultAsync(p => p.Id == pilotId);
        if (delivery.AgreedSubtotal <= 0m && pilot is not null)
        {
            delivery.AgreedSubtotal = Math.Round(pilot.BaseFee + (pilot.PerKmRate * Math.Max(delivery.WeightKg, 1m)), 2);
            delivery.ServiceFee = Math.Round(delivery.AgreedSubtotal * 0.03m, 2);
            delivery.TotalCharge = delivery.AgreedSubtotal + delivery.ServiceFee;
        }

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
        if (delivery is null || (delivery.Status != "awaiting_approval" && delivery.Status != "awaiting_pilot_response")) return null;

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
        delivery.RequestedPilotId = null;
        delivery.RequestedAt = null;
        delivery.AgreedSubtotal = quote.Subtotal;
        delivery.ServiceFee = Math.Round(quote.Subtotal * 0.03m, 2);
        delivery.TotalCharge = delivery.AgreedSubtotal + delivery.ServiceFee;

        await db.SaveChangesAsync();
        return delivery;
    }

    public async Task<Delivery?> RejectQuoteAsync(Guid deliveryId, Guid quoteId)
    {
        var delivery = await db.Deliveries
            .Include(d => d.Quotes)
            .FirstOrDefaultAsync(d => d.Id == deliveryId);

        if (delivery is null) return null;

        var quote = delivery.Quotes.FirstOrDefault(q => q.Id == quoteId);
        if (quote is null) return null;

        quote.Status = "rejected";
        delivery.Status = "awaiting_pilot_response";
        await db.SaveChangesAsync();
        return delivery;
    }
}
