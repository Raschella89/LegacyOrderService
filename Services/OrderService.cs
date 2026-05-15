using LegacyOrderService.Data;
using LegacyOrderService.Models;
using Microsoft.Extensions.Logging;

namespace LegacyOrderService.Services
{
    public sealed class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _logger = logger;
        }

        /// <inheritdoc/>
        /// <exception cref="ProductNotFoundException">
        /// Thrown when <paramref name="productName"/> does not exist in the catalogue.
        /// Callers (e.g. the UI layer) should handle this and present a user-friendly message.
        /// </exception>
        public async Task<Order> PlaceOrderAsync(
            string customerName,
            string productName,
            int quantity,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Looking up price for product {ProductName}", productName);
            var price = await _productRepository.GetPriceAsync(productName, cancellationToken);

            var order = new Order
            {
                CustomerName = customerName,
                ProductName  = productName,
                Quantity     = quantity,
                Price        = price,
            };

            await _orderRepository.SaveAsync(order, cancellationToken);

            _logger.LogInformation(
                "Order placed for {CustomerName}: {Quantity}x {ProductName} totalling {Total:C}",
                customerName, quantity, productName, order.Total);

            return order;
        }
    }
}
