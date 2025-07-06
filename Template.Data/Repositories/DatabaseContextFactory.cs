using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Template.Data.Repositories
{
    /// <summary>
    /// Factory class for creating DatabaseContext instances with configuration
    /// </summary>
    public static class DatabaseContextFactory
    {
        private static IConfiguration _configuration;
        
        /// <summary>
        /// Gets the configuration instance, loading it if necessary
        /// </summary>
        private static IConfiguration Configuration =>
            _configuration ??= new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
        
        /// <summary>
        /// Creates a new DatabaseContext using the specified connection string key
        /// </summary>
        /// <param name="connectionStringKey">The key in appsettings.json (e.g., "Sqlite", "SqlServer", "MySql")</param>
        /// <returns>Configured DatabaseContext instance</returns>
        public static DatabaseContext CreateContext(string connectionStringKey = "Sqlite")
        {
            var connectionString = Configuration.GetConnectionString(connectionStringKey) 
                ?? (connectionStringKey.ToLower() == "sqlite" ? "Filename=data.db" : 
                    throw new InvalidOperationException($"Connection string '{connectionStringKey}' not found in configuration and no default available"));
            
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            
            // Configure based on the connection string key using pattern matching
            optionsBuilder = connectionStringKey.ToLower() switch
            {
                "sqlite" => optionsBuilder.UseSqlite(connectionString),               
                "mysql" => optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)),
                // "sqlserver" => optionsBuilder.UseSqlServer(connectionString),
                // "postgres" => optionsBuilder.UseNpgsql(Configuration.GetConnectionString("Postgres")),
                _ => throw new NotSupportedException($"Database provider for '{connectionStringKey}' is not supported")
            };
            
            return new DatabaseContext(optionsBuilder.Options);
        }
    }
}
