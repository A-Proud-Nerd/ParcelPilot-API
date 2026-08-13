using ParcelPilot.Api.Models;

namespace ParcelPilot.Api.Repositories;

public interface IDeliveryRepository
{
    Task<List<Delivery>> GetAllAsync();
    Task<Delivery?> GetByIdAsync(Guid id);
    Task<Delivery> CreateAsync(Delivery delivery);
    Task<Delivery?> UpdateStatusAsync(Guid id, string status);
    Task<Delivery?> AssignPilotAsync(Guid deliveryId, Guid pilotId);
    Task<Delivery?> RequestPilotAsync(Guid deliveryId, Guid pilotId);
    Task<Delivery?> AcceptRequestAsync(Guid deliveryId, Guid pilotId);
    Task<Delivery?> DeclineRequestAsync(Guid deliveryId, Guid pilotId);
    Task<Quote?> AddQuoteAsync(Guid deliveryId, Quote quote);
    Task<Delivery?> AcceptQuoteAsync(Guid deliveryId, Guid quoteId);
    Task<Delivery?> RejectQuoteAsync(Guid deliveryId, Guid quoteId);
}
