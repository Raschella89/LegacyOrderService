using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LegacyOrderService.Models;
using LegacyOrderService.Options;

namespace LegacyOrderService.Data
{
    public sealed class OrderRepository : IOrderRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<OrderRepository> _logger;

        public OrderRepository(IOptions<DatabaseOptions> options, ILogger<OrderRepository> logger)
        {
            if (string.IsNullOrEmpty(options.Value.ConnectionString))
                throw new InvalidOperationException(
                    $"'{DatabaseOptions.SectionName}:{nameof(DatabaseOptions.ConnectionString)}' is not configured.");

            _connectionString = options.Value.ConnectionString;
            _logger = logger;
            EnsureSchema();
        }

        /// <summary>
        /// Creates the Orders table if it does not already exist.
        /// This ensures the application works correctly on a fresh database without a separate migration step.
        /// </summary>
        private void EnsureSchema()
        {
            _logger.LogDebug("Ensuring Orders schema exists");
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Orders (
                    Id           INTEGER PRIMARY KEY AUTOINCREMENT,
                    CustomerName TEXT    NOT NULL,
                    ProductName  TEXT    NOT NULL,
                    Quantity     INTEGER NOT NULL,
                    Price        TEXT    NOT NULL
                )";
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Persists an order using parameterized queries to prevent SQL injection.
        /// The connection is disposed via <c>await using</c> to prevent connection leaks.
        /// Price is stored as an invariant-culture string to preserve decimal precision
        /// (SQLite has no DECIMAL type; storing as REAL would reintroduce floating-point error).
        /// </summary>
        public async Task SaveAsync(Order order, CancellationToken cancellationToken = default)
        {
            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Orders (CustomerName, ProductName, Quantity, Price)
                VALUES ($customerName, $productName, $quantity, $price)";

            command.Parameters.AddWithValue("$customerName", order.CustomerName);
            command.Parameters.AddWithValue("$productName", order.ProductName);
            command.Parameters.AddWithValue("$quantity", order.Quantity);
            command.Parameters.AddWithValue("$price", order.Price.ToString(System.Globalization.CultureInfo.InvariantCulture));

            await command.ExecuteNonQueryAsync(cancellationToken);

            _logger.LogDebug(
                "Persisted order for customer {CustomerName}, product {ProductName}",
                order.CustomerName, order.ProductName);
        }
    }
}
