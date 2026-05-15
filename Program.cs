using LegacyOrderService.Data;
using LegacyOrderService.Options;
using LegacyOrderService.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<DatabaseOptions>(
            context.Configuration.GetSection(DatabaseOptions.SectionName));

        services.AddSingleton<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderService, OrderService>();
    })
    .Build();

// Resolve scoped services inside an explicit scope so they are disposed correctly.
using var scope = host.Services.CreateScope();
var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

Console.WriteLine("Welcome to Order Processor!");

var customerName = PromptNonEmpty("Enter customer name:");
var productName  = PromptNonEmpty("Enter product name:");
var quantity     = PromptPositiveInt("Enter quantity:");

try
{
    var order = await orderService.PlaceOrderAsync(customerName, productName, quantity);

    Console.WriteLine("\nOrder summary:");
    Console.WriteLine($"  Customer : {order.CustomerName}");
    Console.WriteLine($"  Product  : {order.ProductName}");
    Console.WriteLine($"  Qty      : {order.Quantity}");
    Console.WriteLine($"  Unit     : {order.Price:C}");
    Console.WriteLine($"  Total    : {order.Total:C}");
    Console.WriteLine("\nDone.");
}
catch (ProductNotFoundException ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    Console.Error.WriteLine("Available products: Widget, Gadget, Doohickey");
}

static string PromptNonEmpty(string prompt)
{
    while (true)
    {
        Console.WriteLine(prompt);
        var value = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(value))
            return value;

        Console.Error.WriteLine("Input cannot be empty. Please try again.");
    }
}

static int PromptPositiveInt(string prompt)
{
    while (true)
    {
        Console.WriteLine(prompt);
        var raw = Console.ReadLine();
        if (int.TryParse(raw, out int value) && value > 0)
            return value;

        Console.Error.WriteLine("Please enter a positive whole number.");
    }
}

