using LegacyOrderService.Data;
using LegacyOrderService.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace LegacyOrderService.Tests;

public sealed class OrderServiceTests
{
    private readonly IProductRepository _productRepo = Substitute.For<IProductRepository>();
    private readonly IOrderRepository   _orderRepo   = Substitute.For<IOrderRepository>();
    private readonly OrderService       _sut;

    public OrderServiceTests()
    {
        _sut = new OrderService(
            _orderRepo,
            _productRepo,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<OrderService>.Instance);
    }

    [Fact]
    public async Task PlaceOrderAsync_ValidProduct_ReturnsOrderWithCorrectTotal()
    {
        // Arrange
        _productRepo.GetPriceAsync("Widget", Arg.Any<CancellationToken>())
                    .Returns(12.99m);

        // Act
        var order = await _sut.PlaceOrderAsync("Alice", "Widget", 3);

        // Assert
        Assert.Equal("Alice",   order.CustomerName);
        Assert.Equal("Widget",  order.ProductName);
        Assert.Equal(3,         order.Quantity);
        Assert.Equal(12.99m,    order.Price);
        Assert.Equal(38.97m,    order.Total);  // 3 * 12.99
    }

    [Fact]
    public async Task PlaceOrderAsync_ValidProduct_SavesOrderOnce()
    {
        // Arrange
        _productRepo.GetPriceAsync("Gadget", Arg.Any<CancellationToken>())
                    .Returns(15.49m);

        // Act
        await _sut.PlaceOrderAsync("Bob", "Gadget", 2);

        // Assert — repository must be called exactly once with the built order
        await _orderRepo.Received(1).SaveAsync(
            Arg.Is<Models.Order>(o =>
                o.CustomerName == "Bob" &&
                o.ProductName  == "Gadget" &&
                o.Quantity     == 2 &&
                o.Price        == 15.49m),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PlaceOrderAsync_UnknownProduct_ThrowsProductNotFoundException()
    {
        // Arrange
        _productRepo.GetPriceAsync("Unknown", Arg.Any<CancellationToken>())
                    .Throws(new ProductNotFoundException("Unknown"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ProductNotFoundException>(
            () => _sut.PlaceOrderAsync("Carol", "Unknown", 1));

        Assert.Equal("Unknown", ex.ProductName);
    }

    [Fact]
    public async Task PlaceOrderAsync_UnknownProduct_DoesNotCallSave()
    {
        // Arrange
        _productRepo.GetPriceAsync("Unknown", Arg.Any<CancellationToken>())
                    .Throws(new ProductNotFoundException("Unknown"));

        // Act
        await Assert.ThrowsAsync<ProductNotFoundException>(
            () => _sut.PlaceOrderAsync("Dave", "Unknown", 1));

        // Assert — save must never be called if the product lookup fails
        await _orderRepo.DidNotReceive().SaveAsync(
            Arg.Any<Models.Order>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PlaceOrderAsync_CancellationRequested_PropagatesToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        _productRepo.GetPriceAsync("Widget", Arg.Is<CancellationToken>(t => t.IsCancellationRequested))
                    .Throws<OperationCanceledException>();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _sut.PlaceOrderAsync("Eve", "Widget", 1, cts.Token));
    }
}
