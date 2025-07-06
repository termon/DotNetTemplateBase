using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Template.Data.Repositories;

namespace Template.Data.Extensions
{
    /// <summary>
    /// Extension methods for IServiceCollection to add DatabaseContext
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds DatabaseContext to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The application configuration (optional)</param>
        /// <param name="connectionString">Custom connection string (optional)</param>
        /// <param name="provider">The database provider ("sqlite", "sqlserver", "mysql")</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddDatabaseContext(
            this IServiceCollection services, 
            IConfiguration configuration = null,
            string connectionString = null,
            string provider = "sqlite")
        {
            services.AddDbContext<DatabaseContext>(options =>
            {
                // Determine the connection string to use
                var finalConnectionString = connectionString ?? 
                    configuration?.GetConnectionString(provider) ?? 
                    (provider.ToLower() == "sqlite" ? "Filename=data.db" : 
                     throw new InvalidOperationException($"No connection string found for provider '{provider}' and no default available"));
                
                // Configure based on the provider using pattern matching
                _ = provider.ToLower() switch
                {
                    "sqlite" => options.UseSqlite(finalConnectionString),
                    // Add other providers as needed
                    //"sqlserver" => options.UseSqlServer(finalConnectionString),                  
                    //"postgres" => options.UseNpgsql(configuration.GetConnectionString("postgres")),                           
                    // "mysql" => options.UseMySql(finalConnectionString, ServerVersion.AutoDetect(finalConnectionString)),
                    _ => throw new NotSupportedException($"Database provider '{provider}' is not supported")
                };
            });
            
            return services;
        }
    }
}
