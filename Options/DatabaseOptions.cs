namespace LegacyOrderService.Options
{
    public sealed class DatabaseOptions
    {
        public const string SectionName = "Database";

        /// <summary>SQLite connection string, e.g. "Data Source=orders.db"</summary>
        public string ConnectionString { get; set; } = string.Empty;
    }
}
