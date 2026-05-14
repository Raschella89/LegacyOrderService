using Microsoft.Data.Sqlite;
using LegacyOrderService.Models;

namespace LegacyOrderService.Data
{
    public sealed class OrderRepository : IOrderRepository
    {
        private readonly string _connectionString =
            $"Data Source={Path.Combine(AppContext.BaseDirectory, "orders.db")}";

        public OrderRepository()
        {
            EnsureSchema();
        }

        /// <summary>
        /// Creates the Orders table if it does not already exist.
        /// This ensures the application works correctly on a fresh database without a separate migration step.
        /// </summary>
        private void EnsureSchema()
        {
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
        }
    }
}
