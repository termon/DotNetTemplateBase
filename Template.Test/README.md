# Database Testing Infrastructure

This project provides two different approaches for database testing with SQLite, giving you flexibility to choose the best option for your specific scenarios.

## Base Classes

### `InMemoryDatabaseTestBase`
- **Best for**: Fast tests, simple scenarios, no file system dependencies
- **Features**: 
  - Lightning-fast execution (in-memory)
  - Automatic cleanup (no files created)
  - Full test isolation
  - Parallel execution safe

### `FileDatabaseTestBase`
- **Best for**: Complex scenarios, debugging, data inspection, large datasets
- **Features**:
  - Persistent database files for inspection
  - Automatic cleanup of database files
  - Unique test IDs for debugging
  - Full test isolation
  - Parallel execution safe

## Usage Examples

### In-Memory Database Tests (Recommended for most cases)

```csharp
public class MyServiceTests : InMemoryDatabaseTestBase
{
    private readonly IUserService service;

    public MyServiceTests()
    {
        service = new UserServiceDb(Context);
        service.Initialise();
    }

    [Fact]
    public void MyTest()
    {
        // Your test logic here
        var users = service.GetUsers();
        Assert.Empty(users);
    }
}
```

### File-Based Database Tests (For debugging/inspection)

```csharp
public class MyComplexTests : FileDatabaseTestBase
{
    private readonly IUserService service;

    public MyComplexTests()
    {
        service = new UserServiceDb(Context);
        service.Initialise();
    }

    [Fact]
    public void MyComplexTest()
    {
        // Your test logic here
        var users = service.GetUsers();
        
        // You can inspect the database file at:
        // Console.WriteLine($"Database at: {DatabasePath}");
        
        Assert.Empty(users);
    }
}
```

## Key Benefits

✅ **Isolated**: Each test class gets its own database instance  
✅ **Parallel Safe**: Tests can run in parallel without conflicts  
✅ **Fast**: In-memory option provides maximum speed  
✅ **Debuggable**: File-based option allows database inspection  
✅ **Clean**: Automatic cleanup prevents file accumulation  
✅ **Flexible**: Choose the right approach for each test scenario  

## Migration Guide

If you have existing tests using the old `DatabaseTestBase`, simply change the inheritance:

```csharp
// Old (deprecated)
public class MyTests : DatabaseTestBase

// New (recommended)
public class MyTests : InMemoryDatabaseTestBase
```

The `Context` property and `ResetDatabase()` method work exactly the same way.
