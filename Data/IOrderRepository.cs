using LegacyOrderService.Models;

namespace LegacyOrderService.Data
{
    public interface IOrderRepository
    {
        Task SaveAsync(Order order, CancellationToken cancellationToken = default);
    }
}
