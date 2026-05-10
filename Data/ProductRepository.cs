// Data/ProductRepository.cs
using System.Collections.Frozen;

namespace LegacyOrderService.Data
{
    public sealed class ProductRepository : IProductRepository
    {
        // FrozenDictionary gives the fastest possible read performance for a static catalogue.
        // decimal is used instead of double to avoid floating-point precision errors on money values.
        // OrdinalIgnoreCase ensures "widget" and "Widget" both resolve correctly.
        private static readonly FrozenDictionary<string, decimal> ProductPrices =
            new Dictionary<string, decimal>
            {
                ["Widget"]    = 12.99m,
                ["Gadget"]    = 15.49m,
                ["Doohickey"] = 8.75m,
            }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc/>
        /// <exception cref="ProductNotFoundException">Thrown when the product does not exist.</exception>
        public Task<decimal> GetPriceAsync(string productName, CancellationToken cancellationToken = default)
        {
            if (!ProductPrices.TryGetValue(productName, out var price))
                throw new ProductNotFoundException(productName);

            return Task.FromResult(price);
        }
    }
}

