using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// import the Models (representing structure of tables in database)
using Template.Data.Models; 

namespace Template.Data.Repositories
{
    // The Context is How EntityFramework communicates with the database
    // We define DbSet properties for each table in the database
    public class DatabaseContext : DbContext
    {
         // authentication store
        public DbSet<User> Users { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }
        
        // Configure the context to use Specified database. We are using 
        // Sqlite database as it does not require any additional installations.
        // Template configured to allow use of MySql, SqlServer and Postgres
        // ideally connections strings should be stored in appsettings.json
        // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        // {
        //     optionsBuilder                  
        //         .UseSqlite("Filename=XXX.db")
        //         //.UseMySql("server=localhost; port=3306; database=XXX; user=XXX; password=XXX", new MySqlServerVersion(new Version(8, 0, 31)))
        //         //.UseNpgsql("host=localhost; port=5432; database=XXX; username=XXX; password=XXX")
        //         //.UseSqlServer(@"Server=.\SQLExpress;Database=XXX;Trusted_Connection=True;");
        //         .LogTo(Console.WriteLine, LogLevel.Information) // remove in production
        //         .EnableSensitiveDataLogging()                   // remove in production
        //         ;
        // }

        public static DbContextOptionsBuilder<DatabaseContext> OptionsBuilder => new ();

        // Convenience method to recreate the database thus ensuring the new database takes 
        // account of any changes to Models or DatabaseContext. ONLY to be used in development
        public void Initialise()
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }

    }
}
