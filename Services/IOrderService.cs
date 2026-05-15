using LegacyOrderService.Models;

namespace LegacyOrderService.Services
{
    public interface IOrderService
    {
        Task<Order> PlaceOrderAsync(
            string customerName,
            string productName,
            int quantity,
            CancellationToken cancellationToken = default);
    }
}
