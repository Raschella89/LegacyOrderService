using LegacyOrderService.Data;
using LegacyOrderService.Models;

namespace LegacyOrderService
{
    internal static class Program
    {
        // Entry point wired as async to support async repository calls without
        // blocking the thread pool (.GetAwaiter().GetResult() anti-pattern).
        static async Task Main(string[] args)
        {
            IProductRepository productRepo = new ProductRepository();
            IOrderRepository   orderRepo   = new OrderRepository();

            Console.WriteLine("Welcome to Order Processor!");

            string customerName = PromptNonEmpty("Enter customer name:");
            string productName  = PromptNonEmpty("Enter product name:");

            decimal price;
            try
            {
                price = await productRepo.GetPriceAsync(productName);
            }
            catch (ProductNotFoundException ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Console.Error.WriteLine("Available products: Widget, Gadget, Doohickey");
                return;
            }

            int quantity = PromptPositiveInt("Enter quantity:");

            // Build the order using the price returned from the catalogue —
            // previously the code hardcoded 10.0 here and printed the wrong total.
            var order = new Order
            {
                CustomerName = customerName,
                ProductName  = productName,
                Quantity     = quantity,
                Price        = price,
            };

            Console.WriteLine("\nOrder summary:");
            Console.WriteLine($"  Customer : {order.CustomerName}");
            Console.WriteLine($"  Product  : {order.ProductName}");
            Console.WriteLine($"  Qty      : {order.Quantity}");
            Console.WriteLine($"  Unit     : {order.Price:C}");
            Console.WriteLine($"  Total    : {order.Total:C}");

            Console.WriteLine("\nSaving order...");
            await orderRepo.SaveAsync(order);
            Console.WriteLine("Done.");
        }

        // ---------------------------------------------------------------------------
        // Small helpers that keep Main readable without adding unnecessary abstraction
        // ---------------------------------------------------------------------------

        private static string PromptNonEmpty(string prompt)
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

        private static int PromptPositiveInt(string prompt)
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
    }
}

