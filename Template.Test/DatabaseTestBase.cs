
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Template.Data.Repositories;

namespace Template.Test
{
    /// <summary>
    /// Abstract base class for database tests
    /// Provides common functionality for database testing
    /// </summary>
    public abstract class DatabaseTestBase : IDisposable
    {
        /// <summary>
        /// Gets the database context for this test instance
        /// Use this to create any services that need database access
        /// </summary>
        protected abstract DatabaseContext Context { get; }

        /// <summary>
        /// Resets the database to a fresh state for the next test
        /// Call this at the beginning of each test method if you need a clean database
        /// </summary>
        protected abstract void ResetDatabase();

        public abstract void Dispose();
    }

    /// <summary>
    /// Base class for tests that need in-memory database access
    /// Provides a unique, isolated in-memory database instance for each test class
    /// Fully supports parallel test execution with maximum speed
    /// Best for: Fast tests, simple scenarios, no file system dependencies
    /// </summary>
    public abstract class InMemoryDatabaseTestBase : DatabaseTestBase
    {
        private readonly string connectionString;
        private readonly SqliteConnection connection;
        private DatabaseContext context;

        protected InMemoryDatabaseTestBase()
        {
            // Create an in-memory connection string for this test instance
            connectionString = "Data Source=:memory:";
            
            // Keep a connection open for the lifetime of the test class
            // This prevents the in-memory database from being destroyed
            connection = new SqliteConnection(connectionString);
            connection.Open();
            
            // Initialize the in-memory database context
            InitializeDatabase();
        }

        /// <summary>
        /// Gets the database context for this test instance
        /// Use this to create any services that need database access
        /// </summary>
        protected override DatabaseContext Context => context;

        /// <summary>
        /// Gets the connection string for this test instance
        /// </summary>
        public string ConnectionString => connectionString;

        /// <summary>
        /// Resets the database to a fresh state for the next test
        /// Call this at the beginning of each test method if you need a clean database
        /// </summary>
        protected override void ResetDatabase()
        {
            // Dispose current context
            context?.Dispose();
            
            // Reinitialize the in-memory database
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            // Create context using the existing connection
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseSqlite(connection)
                .Options;
            
            context = new DatabaseContext(options);
            context.Database.EnsureCreated(); // Create the database schema
        }

        public override void Dispose()
        {
            context?.Dispose();
            connection?.Dispose();
        }
    }

    /// <summary>
    /// Base class for tests that need file-based database access
    /// Provides a unique, isolated file-based SQLite database instance for each test class
    /// Fully supports parallel test execution with data persistence
    /// Best for: Complex scenarios, debugging, data inspection, large datasets
    /// </summary>
    public abstract class FileDatabaseTestBase : DatabaseTestBase
    {
        private readonly string testId;
        private readonly string testDbPath;
        private DatabaseContext context;

        protected FileDatabaseTestBase()
        {
            // Create a unique test ID for this specific test instance
            testId = Guid.NewGuid().ToString("N")[..8];
            
            // Create a unique database path for this test instance
            testDbPath = Path.Combine(Path.GetTempPath(), $"xx_test_instance_{testId}.db");
            
            // Initialize the database context
            InitializeDatabase();
        }

        /// <summary>
        /// Gets the database context for this test instance
        /// Use this to create any services that need database access
        /// </summary>
        protected override DatabaseContext Context => context;

        /// <summary>
        /// Gets the unique test ID for this instance
        /// </summary>
        public string TestId => testId;

        /// <summary>
        /// Gets the database file path for this test instance
        /// </summary>
        public string DatabasePath => testDbPath;

        /// <summary>
        /// Resets the database to a fresh state for the next test
        /// Call this at the beginning of each test method if you need a clean database
        /// </summary>
        protected override void ResetDatabase()
        {
            // Dispose current context
            context?.Dispose();
            
            // Delete the database file
            CleanupDatabaseFiles();
            
            // Reinitialize the database
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseSqlite($"Data Source={testDbPath}")
                .Options;
                
            context = new DatabaseContext(options);
            context.Database.EnsureDeleted(); // Ensure clean state
            context.Database.EnsureCreated(); // Ensure the database schema is created
        }

        private void CleanupDatabaseFiles()
        {
            try
            {
                if (File.Exists(testDbPath))
                {
                    File.Delete(testDbPath);
                }
                
                // Clean up SQLite WAL files
                var walFile = testDbPath + "-wal";
                if (File.Exists(walFile))
                {
                    File.Delete(walFile);
                }
                
                // Clean up SQLite shared memory files
                var shmFile = testDbPath + "-shm";
                if (File.Exists(shmFile))
                {
                    File.Delete(shmFile);
                }
            }
            catch
            {
                // Ignore cleanup errors - they're not critical for test success
            }
        }

        public override void Dispose()
        {
            context?.Dispose();
            
            // Clean up the test database files after disposing the context
            CleanupDatabaseFiles();
        }
    }

    /// <summary>
    /// Base class for tests that need production database access (MySQL, PostgreSQL, SQL Server)
    /// Provides a unique, isolated test database instance for each test class
    /// Fully supports parallel test execution with production database providers
    /// Best for: Integration tests, production environment validation, database-specific feature testing
    /// 
    /// NOTE: To use this base class, you need to:
    /// 1. Add the appropriate NuGet packages to the test project
    /// 2. Ensure the production database server is available
    /// 3. Configure connection strings in test settings
    /// </summary>
    public abstract class ProductionDatabaseTestBase : DatabaseTestBase
    {
        private readonly string testId;
        private readonly string databaseProvider;
        private readonly string testDatabaseName;
        private DatabaseContext context;

        protected ProductionDatabaseTestBase(string databaseProvider = "MySQL")
        {
            // Create a unique test ID for this specific test instance
            testId = Guid.NewGuid().ToString("N")[..8];
            this.databaseProvider = databaseProvider;
            
            // Create a unique database name for this test instance
            testDatabaseName = $"xx_test_{testId}";
            
            // Initialize the production database context
            InitializeDatabase();
        }

        /// <summary>
        /// Gets the database context for this test instance
        /// Use this to create any services that need database access
        /// </summary>
        protected override DatabaseContext Context => context;

        /// <summary>
        /// Gets the unique test ID for this instance
        /// </summary>
        public string TestId => testId;

        /// <summary>
        /// Gets the database provider being used
        /// </summary>
        public string DatabaseProvider => databaseProvider;

        /// <summary>
        /// Gets the test database name for this test instance
        /// </summary>
        public string TestDatabaseName => testDatabaseName;

        /// <summary>
        /// Resets the database to a fresh state for the next test
        /// Call this at the beginning of each test method if you need a clean database
        /// </summary>
        protected override void ResetDatabase()
        {
            // Dispose current context
            context?.Dispose();
            
            // Reinitialize the database (this will recreate schema)
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                // Create context using the production database provider
                context = DatabaseContextFactory.CreateContext(databaseProvider);
                
                // Update the context to use our test database name
                UpdateContextForTestDatabase();
                
                // Ensure clean state and create schema
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to initialize production database context for provider '{databaseProvider}'. " +
                    $"Ensure the provider is enabled in DatabaseContextFactory and the database server is available. " +
                    $"Error: {ex.Message}", ex);
            }
        }

        private void UpdateContextForTestDatabase()
        {
            var connectionString = context.Database.GetConnectionString();
            var testConnectionString = UpdateConnectionStringWithTestDatabase(connectionString);
            
            // Dispose the old context and create a new one with the test database
            context.Dispose();
            
            // Create new context with test database connection string
            var options = new DbContextOptionsBuilder<DatabaseContext>();
            
            // Configure based on provider - using MySqlConnector which is already available
            switch (databaseProvider.ToLower())
            {
                case "mysql":
                    options.UseMySql(testConnectionString, ServerVersion.AutoDetect(testConnectionString));
                    break;
                default:
                    throw new NotSupportedException(
                        $"Database provider '{databaseProvider}' is not currently supported in the test project. " +
                        $"To add support, uncomment the package reference in the Data project and add the " +
                        $"corresponding package to the Test project.");
            }
            
            context = new DatabaseContext(options.Options);
        }

        private string UpdateConnectionStringWithTestDatabase(string connectionString)
        {
            return databaseProvider.ToLower() switch
            {
                "mysql" => connectionString.Contains("database=", StringComparison.OrdinalIgnoreCase) 
                    ? System.Text.RegularExpressions.Regex.Replace(connectionString, @"database=[^;]*", $"database={testDatabaseName}", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
                    : connectionString + $";database={testDatabaseName}",
                "postgres" => connectionString.Contains("database=", StringComparison.OrdinalIgnoreCase)
                    ? System.Text.RegularExpressions.Regex.Replace(connectionString, @"database=[^;]*", $"database={testDatabaseName}", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
                    : connectionString + $";database={testDatabaseName}",
                "sqlserver" => connectionString.Contains("Database=", StringComparison.OrdinalIgnoreCase)
                    ? System.Text.RegularExpressions.Regex.Replace(connectionString, @"Database=[^;]*", $"Database={testDatabaseName}", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
                    : connectionString + $";Database={testDatabaseName}",
                _ => connectionString
            };
        }

        public override void Dispose()
        {
            try
            {
                // Clean up the test database
                context?.Database.EnsureDeleted();
            }
            catch (Exception ex)
            {
                // Log but don't fail on cleanup errors
                Console.WriteLine($"Warning: Could not clean up test database: {ex.Message}");
            }
            finally
            {
                context?.Dispose();
            }
        }
    }
}
