using ParcelPilot.Api.Models;

namespace ParcelPilot.Api.Repositories;

public interface IDeliveryRepository
{
    Task<List<Delivery>> GetAllAsync();
    Task<Delivery?> GetByIdAsync(int id);
    Task<Delivery> CreateAsync(Delivery delivery);
    Task<Delivery?> UpdateStatusAsync(int id, string status);
    Task<Quote?> AddQuoteAsync(int deliveryId, Quote quote);
    Task<Delivery?> AcceptQuoteAsync(int deliveryId, int quoteId);
}
