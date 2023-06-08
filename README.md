# DotNet 7 Base MVC Solution

This .NET project ```Template``` provides a solution containing separate Data, Test and Web(MVC) projects. When installed and used to create a new project, all references to Template will be replaced with the name of your project.

## Data Project

The Data project encapsulates all data related concerns and provides data entity (database) and implementations of following services:

1. An implementation of ```Template.Data.Services.IUserService``` using EntityFramework to handle data storage/retrieval is provided via ```Template.Data.Services.UserServiceDb```.
2. An implementation of ```Template.Data.Services.IMailService``` using the .NET Smtp Mail provider to handle email sending is provided via ```Template.Data.Services.SmtpMailService```.

Password hashing functionality is added via the ```Template.Data.Security.Hasher``` class. This is used in the Data project ```UserServiceDb``` to hash the user password before storing in database.

Data pagination is supported via the ```Paged<T>``` entity type. To create paged data from a query we can use the ```ToPaged(...)``` extension method. See ```UserService.GetUsers(....)``` method for a usage example. 

## Test Project

The Test project references the Core and Data projects and should implement unit tests to test any service implementations created in the Data project. A sample test file is provided for implementation of IUserService. You should provide your own tests to exercise the functionality of any services you create.

## Web Project

The Web project uses the MVC pattern to implement a web application. It references the Data project and uses the exposed services and models to access data management functionality. This allows the Web project to be completely independent of the service implementation details defined in the Data project.

### Dependency Injection

The DI container is used to manage creation of services that are consumed in the project and are configured in ```Program.cs```.

The EntityFramework ```DbContext``` can be configured to use either ```Sqlite``` (default), ```MySql```, ```Postgres``` or ```SqlServer``` databases. Connection strings for each database should be configured in ```appsettings.json```. Default configuration example is shown below.

```c#
builder.Services.AddDbContext<DatabaseContext>( options => {
    // Configure connection string for selected database in appsettings.json
    options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));   
});
```

Implementations of ```IUserService``` and ```IMailService``` defined in the data project, ```UserServiceDb``` and ```SmtpMailService``` are also added to the DI container as shown below.

```c#
// Add Application Services to DI   
builder.Services.AddTransient<IUserService,UserServiceDb>();
builder.Services.AddTransient<IMailService,SmtpMailService>();
```

* Database ConnectionString and MailSettings should be configured as required in ```appsettings.json```*

### Identity

The project provides extension methods to enable:

1. User Identity using cookie authentication is enabled without using the boilerplate template used in the standard web projects (mvc,web). This allows the developer to gain a better appreciation of how Identity is implemented. The core project implements a User model and the data project UserService implementation provides user management functionality such as Authenticate, Register, Change Password, Update Profile etc.

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

3. Two custom TagHelpers are included that provide

    a. Authentication and authorisation Tags

    * ```<p asp-authorized>Only displayed if the user is authenticated</p>```

    * ```<p asp-roles="admin,manager">Only displayed if the user has one of specified roles</p>```

    Note: to enable these tag helpers Program.cs needs following service added to DI container
    ```builder.Services.AddHttpContextAccessor();```

    b. Conditional Display Tag

    * ```<p asp-condtion="@some_boolean_expression">Only displayed if the condition is true</p>```

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
 
	* The paginator component can be used to display a UI element to navigate through pages of a model containing a ```Paged<T>``` dataset.

	```c#
	<vc:paginator action="Index" rows=@Model.TotalRows 
                  pages=@Model.TotalPages 
                  current=@Model.CurrentPage 
                  size=@Model.PageSize  />
    ```

 
    * An order component can be used to provide ordering to table columns

    ```c#
    <vc:sort-order field="id" orderby=@Model.OrderBy 
                              direction=@Model.Direction />
    ``` 

    An example of usage for both can be found in the ```UserController.Index``` action and view ```User/Index.cshtml```.

## Install Template

To install this solution as a template (template name is **termonbase**)

1. Download current version of the template

    ```$ git clone https://github.com/termon/DotNetTemplateBase.git```

2. Install the template so it can be used by ```dotnet new``` command. Use the path (i.e the directory location)to the cloned template directory without trailing '/'

    Linux/macOS

    ```$ dotnet new install /path/DotNetTemplateBase```

    Windows

    ```c:> dotnet new install c:\path\DotNetTemplateBase```

3. Once installed you can create a new project using this template.

    ```dotnet new termonbase -n SolutionName```

4. To uninstall a template (no longer can be used with dotnet new ).

    ```dotnet new uninstall /path/DotNetTemplateBase```
