namespace LegacyOrderService.Data
{
    /// <summary>Thrown when a requested product does not exist in the catalogue.</summary>
    public sealed class ProductNotFoundException : Exception
    {
        public string ProductName { get; }

        public ProductNotFoundException(string productName)
            : base($"Product '{productName}' was not found.")
        {
            ProductName = productName;
        }
    }
}
