namespace LegacyOrderService.Data
{
    public interface IProductRepository
    {
        Task<decimal> GetPriceAsync(string productName, CancellationToken cancellationToken = default);
    }
}
