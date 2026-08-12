using ParcelPilot.Api.Models;

namespace ParcelPilot.Api.Repositories;

public interface IDeliveryRepository
{
    Task<List<Delivery>> GetAllAsync();
    Task<Delivery?> GetByIdAsync(Guid id);
    Task<Delivery> CreateAsync(Delivery delivery);
    Task<Delivery?> UpdateStatusAsync(Guid id, string status);
    Task<Quote?> AddQuoteAsync(Guid deliveryId, Quote quote);
    Task<Delivery?> AcceptQuoteAsync(Guid deliveryId, Guid quoteId);
}
