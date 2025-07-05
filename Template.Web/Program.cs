using Template.Web;
using Template.Data.Services;
using Template.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Template.Web;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        // Configure Authentication / Authorisation via extension methods 
        builder.Services.AddCookieAuthentication();

        builder.Services.AddDbContext<DatabaseContext>(options =>
        {
            // Configure connection string for selected database in appsettings.json
            options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));
            //options.UseMySql(builder.Configuration.GetConnectionString("MySql"), ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySql")));
            //options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"));
            //options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
        });

        // Add Application Services to DI   
        builder.Services.AddTransient<IUserService, UserServiceDb>();
        builder.Services.AddTransient<IMailService, SmtpMailService>();

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
            using var scope = app.Services.CreateScope();
            Seeder.Seed(scope.ServiceProvider.GetService<IUserService>());
            //Seeder.SeedDb(scope.ServiceProvider.GetService<DatabaseContext>());
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        // ** turn on authentication/authorisation **
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}
