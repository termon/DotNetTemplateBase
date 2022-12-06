using Template.Web;
using Template.Data.Services;
using Template.Data.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ** Add Cookie Authentication via extension method **
builder.Services.AddCookieAuthentication();

builder.Services.AddDbContext<DatabaseContext>( options => {
    // Configure connection string for selected database in appsettings.json

    options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));   
    //options.UseMySql(builder.Configuration.GetConnectionString("MySql"), ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySql")));
    //options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"));
    //options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
    options.LogTo(Console.WriteLine, LogLevel.Information); // remove in production
    options.EnableSensitiveDataLogging();                   // remove in production
    
});  

// Add UserService to DI   
builder.Services.AddTransient<IUserService,UserServiceDb>();

// ** Required to enable asp-authorize Taghelper **            
builder.Services.AddHttpContextAccessor(); 

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
   app.UseHsts();
}
else 
{
    // seed users in development mode - using service provider to get UserService from DI
    using (var scope = app.Services.CreateScope())
    {
       Seeder.Seed(scope.ServiceProvider.GetService<IUserService>());
    }
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ** configure cors to allow full cross origin access to any webapi end points **
//app.UseCors(c => c.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

// ** turn on authentication/authorisation **
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
