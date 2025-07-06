# DotNet 9 Base MVC Solution

This .NET project ```Template``` provides a solution containing separate Data, Test and Web(MVC) projects. When installed and used to create a new project, all references to ```Template``` will be replaced with the name of your project.

## Data Project

The Data project encapsulates all data related concerns and provides data entity (database) and implementations of following services:

1. An implementation of ```Template.Data.Services.IUserService``` using EntityFramework to handle data storage/retrieval is provided via ```Template.Data.Services.UserServiceDb```.
2. An implementation of ```Template.Data.Services.IMailService``` using the .NET Smtp Mail provider to handle email sending is provided via ```Template.Data.Services.SmtpMailService```.

Password hashing functionality is added via the ```Template.Data.Security.Hasher``` class. This is used in the Data project ```UserServiceDb``` to hash the user password before storing in database.

Data pagination is supported via the ```Paged<T>``` data-type. To create paged data from a query we can use the ```ToPaged(...)``` extension method. See ```UserService.GetUsers(....)``` method for a usage example.

### Database Context Factory

The Data project includes a `DatabaseContextFactory` class that provides a clean, configuration-based approach to creating database contexts for production use. It supports multiple database providers:

- **SQLite** (default) - Ready to use
- **MySQL** - Ready to use
- **PostgreSQL** - Available (commented out)
- **SQL Server** - Available (commented out)

Example usage:
```csharp
// Use default SQLite
var context = DatabaseContextFactory.CreateContext();

// Use specific database provider
var context = DatabaseContextFactory.CreateContext("MySQL");
```

#### Enabling Additional Database Providers

PostgreSQL and SQL Server support is included in the project but commented out by default. To enable these providers, follow these simple steps:

**Step 1: Enable the NuGet Package**

In `Template.Data/Template.Data.csproj`, uncomment the package reference you need:

For PostgreSQL:
```xml
<!-- Change this line: -->
<!-- <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="..." /> -->

<!-- To this: -->
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="..." />
```

For SQL Server:
```xml
<!-- Change this line: -->
<!-- <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="..." /> -->

<!-- To this: -->
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="..." />
```

**Step 2: Restore Packages**

After uncommenting the package reference, restore the packages:
```bash
dotnet restore
```

**Step 3: Uncomment the Code**

In `Template.Data/Repositories/DatabaseContextFactory.cs`, uncomment the relevant lines:

For PostgreSQL:
```csharp
// Change this line:
// "postgres" => optionsBuilder.UseNpgsql(connectionString),

// To this:
"postgres" => optionsBuilder.UseNpgsql(connectionString),
```

For SQL Server:
```csharp
// Change this line:
// "sqlserver" => optionsBuilder.UseSqlServer(connectionString),

// To this:
"sqlserver" => optionsBuilder.UseSqlServer(connectionString),
```

**Step 4: Update Connection String**

The connection strings are already configured in `appsettings.json` but need to be updated with your actual database details:

```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=(localdb)\\mssqllocaldb;Database=XXX;Trusted_Connection=True;",
    "MySql":     "server=localhost; port=3306; database=XXX; user=XXX; password=XXX",
    "Postgres":  "host=localhost; port=5432; database=XXX; username=XXX; password=XXX",
    "Sqlite":    "Filename=data.db",
    "Default":   "Filename=data.db"
  }
}
```

**Update the placeholders:**
- Replace `XXX` with your actual database name
- Replace `XXX` in usernames/passwords with your actual credentials
- Update server addresses, ports, and other settings as needed for your environment

**Step 5: Use the New Provider**

```csharp
// In Program.cs or wherever you create the context
var context = DatabaseContextFactory.CreateContext("Postgres");
// or
var context = DatabaseContextFactory.CreateContext("SqlServer");
```

> **Note**: Remember to update your connection strings with your actual database server details. The examples above use localhost with default settings.

The factory reads connection strings from `appsettings.json` and provides fallback defaults for common scenarios. Test database contexts are now handled entirely by the test infrastructure base classes for better separation of concerns. 

## Test Project

The Test project references the Core and Data projects and provides a comprehensive testing infrastructure with isolated, parallel-safe database testing capabilities.

### Database Testing Infrastructure

The test project provides three flexible base classes for database testing:

#### `InMemoryDatabaseTestBase` (Recommended)
- **Best for**: Fast tests, simple scenarios, no file system dependencies
- **Features**: 
  - Lightning-fast execution (in-memory SQLite)
  - Automatic cleanup (no files created)
  - Full test isolation per test class
  - Parallel execution safe
  - Zero configuration required

#### `FileDatabaseTestBase`
- **Best for**: Complex scenarios, debugging, data inspection, large datasets
- **Features**:
  - Persistent database files for inspection
  - Automatic cleanup of database files (including WAL/SHM files)
  - Unique test IDs for debugging
  - Full test isolation per test class
  - Parallel execution safe

#### `ProductionDatabaseTestBase`
- **Best for**: Integration tests, production environment validation, database-specific feature testing
- **Features**:
  - Tests against actual production database providers (MySQL, PostgreSQL, SQL Server)
  - Unique test database creation per test class
  - Automatic database cleanup after tests
  - Full test isolation per test class
  - Parallel execution safe
  - Validates production-specific database behavior
  - Provides test database information for debugging (`TestId`, `TestDatabaseName`, `DatabaseProvider`)

> **Current Support**: MySQL is fully supported. PostgreSQL and SQL Server support is available but requires additional setup (see enabling steps below).

### Usage Examples

**In-Memory Database Tests (Recommended for most cases):**

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

**File-Based Database Tests (For debugging/inspection):**

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
        // Your test logic here - database file can be inspected at DatabasePath
        var users = service.GetUsers();
        Assert.Empty(users);
    }
}
```

**Production Database Tests (For integration testing):**

```csharp
public class MyIntegrationTests : ProductionDatabaseTestBase
{
    private readonly IUserService service;

    public MyIntegrationTests() : base("MySQL") // or "Postgres", "SqlServer"
    {
        service = new UserServiceDb(Context);
        service.Initialise();
    }

    [Fact]
    public void MyProductionTest()
    {
        // Your test logic here - tests against actual MySQL database
        var users = service.GetUsers();
        Assert.Empty(users);
        
        // Available properties for debugging:
        // TestId - unique identifier for this test instance
        // TestDatabaseName - name of the test database created
        // DatabaseProvider - the database provider being used
        Console.WriteLine($"Test ID: {TestId}");
        Console.WriteLine($"Test Database: {TestDatabaseName}");
        Console.WriteLine($"Provider: {DatabaseProvider}");
    }
}
```

#### Enabling Production Database Testing

To use `ProductionDatabaseTestBase`, you need to:

**Step 1: Enable the Database Provider**
Follow the steps in the [Database Context Factory](#database-context-factory) section to enable the desired database provider in the Data project.

**Step 2: Ensure Database Server Access**
- Make sure the database server (MySQL, PostgreSQL, or SQL Server) is running and accessible
- Update the connection string in `appsettings.json` with valid credentials
- Ensure the database user has permissions to create and drop databases

**Step 3: Use the Production Test Base**
Inherit from `ProductionDatabaseTestBase` and specify the database provider in the constructor.

> **Note**: If you encounter a "Database provider not supported" error, it means the provider hasn't been enabled in the test project. Follow the steps above to enable the desired provider.

### Integration Tests

Test project also supports integration tests to verify operation of controllers. See `HomeControllerIntegrationTests` for example.

### Key Testing Benefits

✅ **Isolated**: Each test class gets its own database instance  
✅ **Parallel Safe**: Tests can run in parallel without conflicts  
✅ **Fast**: In-memory option provides maximum speed  
✅ **Debuggable**: File-based option allows database inspection  
✅ **Production Ready**: Production database option validates real-world scenarios  
✅ **Clean**: Automatic cleanup prevents file/database accumulation  
✅ **Flexible**: Choose the right approach for each test scenario

### When to Use Each Approach

- **`InMemoryDatabaseTestBase`**: Unit tests, service layer tests, fast feedback loops
- **`FileDatabaseTestBase`**: Complex scenarios requiring debugging, data inspection
- **`ProductionDatabaseTestBase`**: Integration tests, database-specific feature validation, pre-deployment testing

### Running Tests

#### Console
> ```dotnet test``` run all tests
> ```dotnet test --filter "ResetPasswordRequests_WhenAllCompleted_ShouldExpireAllTokens" --verbosity normal``` run specific tests

## Web Project

The Web project uses the MVC pattern to implement a web application. It references the Data project and uses the exposed services and models to access data management functionality. This allows the Web project to be completely independent of the service implementation details defined in the Data project.

### Dependency Injection

The DI container is used to manage creation of services that are consumed in the project and are configured in ```Program.cs```.

The EntityFramework ```DbContext``` can be configured to use either ```Sqlite``` (default), ```MySql```, ```Postgres``` or ```SqlServer``` databases. Connection strings for each database should be configured in ```appsettings.json```. 

#### Database Configuration

The project provides a convenient extension method to configure the database context:

```c#
// Simple configuration using appsettings.json (recommended)
builder.Services.AddDatabaseContext(builder.Configuration);
```

This extension method:
- Automatically reads connection strings from `appsettings.json`
- Defaults to SQLite for maximum compatibility
- Can be configured to use any of the four supported database providers

#### Selecting Database Providers

**Option 1: Using appsettings.json (Recommended)**

Configure your desired database provider by updating the connection string keys in `appsettings.json` as outlined above.

Then specify the provider in your `AddDatabaseContext` call:

```c#
// Use MySQL
builder.Services.AddDatabaseContext(builder.Configuration, provider: "mysql");

// Use PostgreSQL  
builder.Services.AddDatabaseContext(builder.Configuration, provider: "postgres");

// Use SQL Server
builder.Services.AddDatabaseContext(builder.Configuration, provider: "sqlserver");

// Use SQLite (default)
builder.Services.AddDatabaseContext(builder.Configuration); // or provider: "sqlite"
```

**Option 2: Direct Connection String**

You can also provide a connection string directly:

```c#
builder.Services.AddDatabaseContext(
    connectionString: "your-connection-string-here",
    provider: "mysql"
);
```

**Option 3: Manual Configuration (Legacy)**

For more control, you can still configure the context manually:

```c#
builder.Services.AddDbContext<DatabaseContext>( options => {
    // Configure connection string for selected database in appsettings.json
    options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));   
});
```

Or use the factory directly:

```c#
builder.Services.AddScoped<DatabaseContext>(_ => 
    DatabaseContextFactory.CreateContext("Sqlite"));
```

#### Enabling Additional Database Providers

To use PostgreSQL or SQL Server with the extension method:

1. **Enable the provider in the Data project** - Follow the steps in the [Database Context Factory](#database-context-factory) section
2. **Enable the provider in the extension method** - In `Template.Data/Extensions/ServiceCollectionExtensions.cs`, uncomment the relevant line:

For PostgreSQL:
```c#
// Change this line:
//"postgres" => options.UseNpgsql(configuration.GetConnectionString("postgres")),

// To this:
"postgres" => options.UseNpgsql(finalConnectionString),
```

For SQL Server:
```c#
// Change this line:
//"sqlserver" => options.UseSqlServer(finalConnectionString),

// To this:
"sqlserver" => options.UseSqlServer(finalConnectionString),
```

For MySQL (already enabled):
```c#
// Change this line:
// "mysql" => options.UseMySql(finalConnectionString, ServerVersion.AutoDetect(finalConnectionString)),

// To this:
"mysql" => options.UseMySql(finalConnectionString, ServerVersion.AutoDetect(finalConnectionString)),
```

3. **Update your connection strings** in `appsettings.json` with your actual database server details

#### Application Services

Implementations of ```IUserService``` and ```IMailService``` defined in the data project, ```UserServiceDb``` and ```SmtpMailService``` are also added to the DI container as shown below.

```c#
// Add Application Services to DI   
builder.Services.AddTransient<IUserService,UserServiceDb>();
builder.Services.AddTransient<IMailService,SmtpMailService>();
```

> **Note**: Database ConnectionString and MailSettings should be configured as required in ```appsettings.json``` ```SmtpMailService``` are also added to the DI container as shown below.

```c#
// Add Application Services to DI   
builder.Services.AddTransient<IUserService,UserServiceDb>();
builder.Services.AddTransient<IMailService,SmtpMailService>();
```

* Database ConnectionString and MailSettings should be configured as required in ```appsettings.json```*

### Identity

The project provides extension methods to enable:

1. User Identity using cookie authentication is enabled without using the boilerplate Template used in the standard web projects (mvc,web). This allows the developer to gain a better appreciation of how Identity is implemented. The core project implements a User model and the data project UserService implementation provides user management functionality such as Authenticate, Register, Change Password, Update Profile etc.

The Web project implements a UserController with actions for Login/Register/NotAuthorized/NotAuthenticated etc. These are implemented using the ```IUserService``` outlined above. The ```AuthBuilder``` helper class defined in ```Template.Web.Helpers``` provides a ```BuildClaimsPrinciple``` method to build a set of user claims for User Login action when using cookie authentication and this can be modified to amend the claims added to the cookie.

To enable cookie Authentication the following statement is included in Program.cs.

```c#
builder.Services.AddCookieAuthentication();
```

Then Authentication/Authorisation are then turned on in the Application via the following statements in Program.cs

```c#
app.UseAuthentication();
app.UseAuthorization();
```

### Additional Functionality

1. Any Controller that inherits from the Web project BaseController, can utilise:

    a. The Alert functionality. Alerts can be used to display alert messages following controller actions. Review the UserController for an example using alerts.

    ```Alert("The User Was Registered Successfully", AlertType.info);```

2. A ClaimsPrincipal authentication extension method
    * ```User.GetSignedInUserId()``` - returns Id of current logged in user or 0 if not logged in

3. Custom TagHelpers are included that provide

    a. Conditional Display Tag

    * ```<p asp-condtion="@some_boolean_expression">Only displayed if the condition is true</p>```

    Note: this can be used with claims principal extension method above to conditionally hide/display UI elements depending on whether a user has a specific role as shown below:

    ```
    <div asp-condtion="@User.HasOneOfRoles("rolea, roleb")"> ... </div>
    ```

4. A Breadcrumbs partial view is contained in ```Views/Shared/_Breadcrumbs.cshtml``` and can be added to a View as shown in example below. The the model parameter is an array of tuples containing the route and breadcrumb.

    ```c#
    <partial name="_BreadCrumbs" model=@(new [] {
        ("/","Home"),
        ("/student","Students"),
        ($"/student/details/{Model.Id}",$"{Model.Id}"),
        ("","Details")
    }) />
    ```

5. View components are provided for ordering and paging of tabular data sets.  
 
	* The paginator component can be used to display a UI element to navigate through pages of a model containing a ```Paged<T>``` dataset. The paginator has a single required ```pages``` parameter which is provided via ```@Model.Pages```. The component also accepts a second optional ```links``` parameter which can be used to configure the number of page links (default is 15)

	```c#
	<vc:paginator pages=@Model.Pages links="10" />
    ```

    * A sort link component can be used to provide ordering to table columns. In example below the component provides an anchor tag that will sort the data by "id" column.

    ```c#
    <vc:sort-link column="id" />
    ``` 

    An example of usage for both can be found in the ```UserController.Index``` action and view ```User/Index.cshtml```.

### Modern JavaScript Framework Support

The project includes support for modern JavaScript frameworks:

#### Alpine.js Support

Alpine.js is a lightweight JavaScript framework that provides reactive data binding and component functionality. It's loaded by default in the layout for consistent functionality across all views.

**Using Alpine.js in views:**

Alpine.js is available on all pages, so you can use it directly in any view:

```razor
<div x-data="{ open: false, message: 'Hello World' }">
    <button @click="open = !open" class="btn btn-primary">
        Toggle Content
    </button>
    <div x-show="open" x-transition>
        <p x-text="message"></p>
    </div>
</div>
```

**For advanced Alpine.js initialization:**

If you need custom Alpine.js initialization code, you can add it to your view:

```razor
@section AlpineJS {
    <script>
        // Alpine.js specific initialization code
        document.addEventListener('alpine:init', () => {
            Alpine.data('myComponent', () => ({
                message: 'Custom component data'
            }));
        });
    </script>
}
```

#### HTMX Support

HTMX enables modern web interactions without writing JavaScript, using HTML attributes to make AJAX requests and update page content dynamically. It's loaded only when needed by specific views.

**To enable HTMX in a view:**

1. Add the HTMX section to your view:
```razor
@section HTMX {
    <script>
        // Optional: HTMX specific configuration
        htmx.config.defaultSwapStyle = 'outerHTML';
        
        // Optional: HTMX event handlers
        document.body.addEventListener('htmx:afterSwap', function(evt) {
            // Handle after content swap
        });
    </script>
}
```

2. Use HTMX attributes in your view content:
```razor
<div>
    <button hx-get="/Home/GetData" 
            hx-target="#result" 
            hx-swap="innerHTML"
            class="btn btn-primary">
        Load Data
    </button>
    <div id="result">
        <!-- Content will be loaded here -->
    </div>
</div>
```

#### Using Both Alpine.js and HTMX

You can use both frameworks in the same view when needed:

```razor
@section HTMX {
    <script>
        // HTMX configuration
    </script>
}

@section AlpineJS {
    <script>
        // Alpine.js custom initialization
    </script>
}

<div x-data="{ loading: false }">
    <button hx-get="/api/data" 
            hx-target="#content"
            @click="loading = true"
            hx-on::after-request="loading = false"
            class="btn btn-primary">
        <span x-show="!loading">Load Data</span>
        <span x-show="loading">Loading...</span>
    </button>
    <div id="content"></div>
</div>
```

#### Alert System Integration

The project's alert system uses Alpine.js for enhanced functionality:

- **Auto-dismiss**: Alerts automatically disappear after 5 seconds
- **Smooth transitions**: Fade and scale animations when dismissing
- **Manual dismiss**: Users can still click the close button
- **Accessible**: Proper ARIA attributes and keyboard support

#### Benefits of This Approach

- **Alpine.js Always Available**: No need to conditionally load Alpine.js - it's ready for use on any page
- **HTMX On-Demand**: HTMX is only loaded when needed for optimal performance
- **Enhanced User Experience**: Alerts and interactive elements work consistently
- **Progressive Enhancement**: Views work without JavaScript and are enhanced when libraries are available
- **Developer Friendly**: Easy to add interactivity without complex JavaScript builds

> **Note**: The layout includes jQuery for form validation compatibility. Alpine.js provides a modern alternative for new interactive features.

## Install Template

To install this solution as a Template (Template name is **termonbase**)

1. Download current version of the Template

    ```$ git clone https://github.com/termon/DotNetTemplateBase.git```

2. Install the Template so it can be used by ```dotnet new``` command. Use the path (i.e the directory location)to the cloned Template directory without trailing '/'

    Linux/macOS

    ```$ dotnet new install /path/DotNetTemplateBase```

    Windows

    ```c:> dotnet new install c:\path\DotNetTemplateBase```

3. Once installed you can create a new project using this Template.

    ```dotnet new termonbase -n SolutionName```

4. To uninstall a Template (no longer can be used with dotnet new ).

    ```dotnet new uninstall /path/DotNetTemplateBase```
