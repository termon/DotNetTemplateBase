using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// import the Entities (database models representing structure of tables in database)
using Template.Data.Entities; 

namespace Template.Data.Repositories
{
    // The Context is How EntityFramework communicates with the database
    // We define DbSet properties for each table in the database
    public class DatabaseContext : DbContext
    {
         // authentication store
        public DbSet<User> Users { get; set; }
        public DbSet<ForgotPassword> ForgotPasswords { get; set; }

        // add any additional DbSet properties for other entities here



        // Default constructor for scenarios where no DI is available
        public DatabaseContext() : base() { }
        
        // Constructor that accepts DbContextOptions, typically used with Dependency Injection
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }
        
        // Configure the context with default options when not already configured via DI
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Filename=data.db");                
                optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information).EnableSensitiveDataLogging();
            }
        }


        public static DbContextOptionsBuilder<DatabaseContext> OptionsBuilder => new();

        // Convenience method to recreate the database thus ensuring the new database takes 
        // account of any changes to Models or DatabaseContext. ONLY to be used in development
        public void Initialise()
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }

    }
}
